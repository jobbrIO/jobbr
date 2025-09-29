using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Jobbr.Server.WebAPI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Container = SimpleInjector.Container;

namespace Jobbr.Dashboard.Tests
{
    [TestClass]
    public class DashboardBackendTests
    {
        private Container _serviceContainer;

        [TestInitialize]
        public void Startup()
        {
            _serviceContainer = new Container();
            _serviceContainer.Register<JobbrWebApiConfiguration>();
            _serviceContainer.Register<DashboardConfiguration>();
        }

        [TestMethod]
        public async Task DashboardBackend_Start_ConfigUrlIsAvailable()
        {
            // Arrange
            var config = new DashboardConfiguration
            {
                BackendAddress = $"http://localhost:{TcpPortHelper.NextFreeTcpPort()}"
            };

            var host = new DashboardBackend(new NullLoggerFactory(), _serviceContainer, config);
            host.Start();

            // Act
            var response = await new HttpClient().GetAsync(config.BackendAddress + "/config");

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
        }

        [TestMethod]
        public async Task DashboardBackend_AfterStop_IsNotAvailable()
        {
            // Arrange
            var config = new DashboardConfiguration
            {
                BackendAddress = $"http://localhost:{TcpPortHelper.NextFreeTcpPort()}"
            };

            var host = new DashboardBackend(new NullLoggerFactory(), _serviceContainer, config);

            host.Start();
            host.Stop();

            // Act
            // Assert
            var ex = await Should.ThrowAsync<Exception>(() => new HttpClient().GetAsync(config.BackendAddress + "/config"));
            ex.InnerException.ShouldBeOfType<SocketException>();
        }

        [TestMethod]
        public void DashboardBackend_StartedTwice_RaisesException()
        {
            // Arrange
            var config = new DashboardConfiguration
            {
                BackendAddress = $"http://localhost:{TcpPortHelper.NextFreeTcpPort()}"
            };

            var host = new DashboardBackend(new NullLoggerFactory(), _serviceContainer, config);

            // Act
            // Assert
            host.Start();

            Assert.Throws<InvalidOperationException>(() => host.Start());
        }

        [TestMethod]
        public async Task DashboardBackend_AfterDisposal_IsNotAvailable()
        {
            // Arrange
            var config = new DashboardConfiguration
            {
                BackendAddress = $"http://localhost:{TcpPortHelper.NextFreeTcpPort()}"
            };

            var host = new DashboardBackend(new NullLoggerFactory(), _serviceContainer, config);

            host.Start();
            host.Dispose();

            // On Linux the disposal seems to take a bit longer
            await Task.Delay(TimeSpan.FromMilliseconds(200));

            var ex = await Should.ThrowAsync<Exception>(() => new HttpClient().GetAsync(config.BackendAddress + "/config"));
            ex.InnerException.ShouldBeOfType<SocketException>();
        }
    }
}
