using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Polly;

namespace CircuitBreakerSandbox
{
    class Program
    {
        private static int _count = 0;

        static void Main(string[] args)
        {
            try
            {
                for (; _count < 10; _count++)
                {
                    GetDataFromCache();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        private static void GetDataFromCache()
        {
            var policy = Policy
                .Handle<Exception>()
                //.Retry(3);
                .WaitAndRetry(2, i => TimeSpan.FromSeconds(5));

            Action<Exception, TimeSpan, Context> onBreak = (exception, timespan, context) =>
            {
                // turn off redis
                Debug.WriteLine("Could not reach cache server...");
            };
            Action<Context> onReset = context =>
            {
                Debug.WriteLine("Cache server is back online...");
            };

            var breaker = Policy
                .Handle<Exception>()
                .CircuitBreaker(2, TimeSpan.FromMinutes(2), onBreak, onReset);

            breaker.Execute(GetItem);
        }

        private static void GetItem()
        {
            Debug.WriteLine("Read from cache.");

            if (_count == 5)
            {
                throw new Exception("Redis not available...");
            }
        }
    }
}
