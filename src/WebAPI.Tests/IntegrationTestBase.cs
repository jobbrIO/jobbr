using System.Net;
using System.Net.Sockets;
using Jobbr.ComponentModel.JobStorage;
using Jobbr.ComponentModel.Registration;
using Jobbr.Server;
using Jobbr.Server.Builder;
using Jobbr.Server.WebAPI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jobbr.WebAPI.Tests
{
    public class IntegrationTestBase
    {
        public string BackendAddress { get; private set; }

        public IJobStorageProvider JobStorage => ExposeStorageProvider.Instance.JobStorageProvider;

        public ServiceProvider ServiceProvider { get; private set; }

        public static int NextFreeTcpPort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            var port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        protected JobbrServer GivenRunningServerWithWebApi(string url = "")
        {
            ServiceProvider = new ServiceCollection()
                .AddLogging(b => b.AddDebug().AddConsole())
                .BuildServiceProvider();

            var loggingFactory = ServiceProvider.GetService<ILoggerFactory>();

            var builder = new JobbrBuilder(loggingFactory);

            if (string.IsNullOrWhiteSpace(url))
            {
                var nextTcpPort = NextFreeTcpPort();
                BackendAddress = $"http://localhost:{nextTcpPort}";
            }
            else
            {
                BackendAddress = url;
            }

            builder.AddWebApi(conf =>
            {
                conf.BackendAddress = BackendAddress;
            });

            builder.RegisterForCollection<IJobbrComponent>(typeof(ExposeStorageProvider));

            var server = builder.Create();

            server.Start();

            return server;
        }

        protected string CreateUrl(string path)
        {
            return $"{BackendAddress}/{path}";
        }
    }
}