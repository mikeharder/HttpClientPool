using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientPool
{
    class Program
    {
        static void Main(string[] args)
        {
            new WebHostBuilder()
                .UseKestrel()
                .Configure(app => app.Run(async (context) =>
                {
                    await context.Response.WriteAsync("Hello World!");
                }))
                .Build()
                .Start();

            var pool = new HttpClientPool(4);
            pool.BaseAddress = new Uri("http://localhost:5000");

            var tasks = new Task[10];
            for (var i=0; i < tasks.Length; i++)
            {
                tasks[i] = ExecuteRequests(pool);
            }

            while (true)
            {
                Console.WriteLine($"Pool: {pool}");
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        private static async Task ExecuteRequests(HttpClientPool pool)
        {
            while (true)
            {
                var client = pool.GetInstance();
                try
                {
                    await client.GetAsync("/");                    
                }
                finally
                {
                    pool.ReturnInstance(client);
                }
            }
        }
    }
}