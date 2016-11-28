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
                GetDataFromCache();
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
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
            };
            Action<Context> onReset = context =>
            {

            };

            var breaker = Policy
                .Handle<Exception>()
                .CircuitBreaker(1, TimeSpan.FromMinutes(2), onBreak, onReset);

            breaker.Execute(GetItem);
        }

        private static void GetItem()
        {
            throw new Exception("Redis not available...");
        }
    }
}
