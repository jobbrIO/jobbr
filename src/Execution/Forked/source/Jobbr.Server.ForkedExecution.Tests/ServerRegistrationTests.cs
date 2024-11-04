using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Jobbr.Server.Builder;
using Jobbr.Server.ForkedExecution.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.Server.ForkedExecution.Tests
{
    [TestClass]
    public class ServerRegistrationTests
    {
        [TestMethod]
        public void WithInMemoryServer_ServerHasStarted_StatusEndpointIsAvailable()
        {
            var backendAddress = "http://localhost:" + TcpPortHelper.NextFreeTcpPort();

            var builder = new JobbrBuilder(NullLoggerFactory.Instance);
            builder.AddForkedExecution(config =>
            {
                config.BackendAddress = backendAddress;
                config.JobRunDirectory = Path.GetTempPath();
                config.JobRunnerExecutable = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe") : "/bin/sh";
            });

            builder.Add<IServiceCollection>(new ServiceCollection());

            var server = builder.Create();

            server.Start();

            var statusResponse = new HttpClient().GetAsync(backendAddress + "/fex/status").Result;

            Assert.AreEqual(HttpStatusCode.OK, statusResponse.StatusCode);
        }

        [TestMethod]
        [Ignore("Setting the port on Jobbr.Server should be debugged.")]
        public void WithInMemoryServer_InvalidPort_ServerStartFails()
        {
            var nextFreeTcpPort = TcpPortHelper.NextFreeTcpPort();

            // intentionally block port
            var listener = new TcpListener(IPAddress.Any, nextFreeTcpPort);
            listener.Start();

            var backendAddress = "http://localhost:" + nextFreeTcpPort;

            var builder = new JobbrBuilder(NullLoggerFactory.Instance);
            builder.AddForkedExecution(config =>
            {
                config.BackendAddress = backendAddress;
                config.JobRunDirectory = Path.GetTempPath();
                config.JobRunnerExecutable = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe") : "/bin/sh";
            });

            builder.Add<IServiceCollection>(new ServiceCollection());

            var server = builder.Create();
            try
            {
                server.Start();
            }
            catch
            {
                // ignored
            }

            Assert.IsNotNull(listener);
            Assert.AreEqual(JobbrState.Error, server.State, "The server should be in error state if a component didn't start");
        }
    }
}
