using BeetleX.FastHttpApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TcpPerformance
{
    class Program
    {


        static void Main(string[] args)
        {

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<HttpServerHosted>();
                });
            builder.Build().Run();
        }


        public class HttpServerHosted : IHostedService
        {
            private HttpApiServer mApiServer;

            public virtual Task StartAsync(CancellationToken cancellationToken)
            {
                BeetleX.Buffers.BufferPool.BUFFER_SIZE = 1024 * 2;
                BeetleX.Buffers.BufferPool.POOL_MAX_SIZE = 1024 * 1024 * 2;
                TcpBanchmarks.Service.Default.Load();
                mApiServer = new HttpApiServer();
                mApiServer.Register(typeof(Program).Assembly);
                mApiServer.Options.Debug = true;
                mApiServer.Open();
                mApiServer.Log(BeetleX.EventArgs.LogType.Info, $"Tcp benchmark started[{typeof(Program).Assembly.GetName().Version}]");
                return Task.CompletedTask;
            }

            public virtual Task StopAsync(CancellationToken cancellationToken)
            {
                mApiServer.Dispose();
                return Task.CompletedTask;
            }
        }
    }
}
