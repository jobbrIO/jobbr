using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Jobbr.Server.ForkedExecution.BackChannel;
using Jobbr.Server.ForkedExecution.Tests.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector;

namespace Jobbr.Server.ForkedExecution.Tests
{
    [TestClass]
    public class BackChannelWebHostTests
    {
        private LoggerFactory _loggerFactory;

        [TestInitialize]
        public void Startup()
        {
            _loggerFactory = new LoggerFactory();
        }

        [TestMethod]
        public void BackChannel_StartWebHost_StatusUrlIsAvailable()
        {
            // Arrange
            var forkedExecutionConfiguration = new ForkedExecutionConfiguration
            {
                BackendAddress = "http://localhost:" + TcpPortHelper.NextFreeTcpPort()
            };

            var host = new BackChannelWebHost(_loggerFactory, new Container(), forkedExecutionConfiguration);
            host.Start();

            // Act
            var response = new HttpClient().GetAsync(forkedExecutionConfiguration.BackendAddress + "/fex/status").Result;

            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task BackChannel_AfterStop_IsNotAvailable()
        {
            // Arrange
            var forkedExecutionConfiguration = new ForkedExecutionConfiguration
            {
                BackendAddress = "http://localhost:" + TcpPortHelper.NextFreeTcpPort()
            };

            var host = new BackChannelWebHost(_loggerFactory, new Container(), forkedExecutionConfiguration);

            host.Start();
            host.Stop();

            try
            {
                // Act
                await new HttpClient().GetAsync(forkedExecutionConfiguration.BackendAddress + "/fex/status");
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SocketException == false)
                {
                    // Assert
                    Assert.Fail("Exception thrown was " + ex.InnerException + ", which is not the expected exception");
                }
            }
        }

        [TestMethod]
        public async Task BackChannel_AfterDisposal_IsNotAvailable()
        {
            // Arrange
            var forkedExecutionConfiguration = new ForkedExecutionConfiguration
            {
                BackendAddress = "http://localhost:" + TcpPortHelper.NextFreeTcpPort()
            };

            var host = new BackChannelWebHost(_loggerFactory, new Container(), forkedExecutionConfiguration);

            host.Start();
            host.Dispose();

            try
            {
                // Act
                await new HttpClient().GetAsync(forkedExecutionConfiguration.BackendAddress + "/fex/status");
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SocketException == false)
                {
                    // Assert
                    Assert.Fail("Exception thrown was " + ex.InnerException + ", which is not the expected exception");
                }
            }
        }

        [TestMethod]
        public void BackChannel_StartedTwice_RaisesException()
        {
            // Arrange
            var forkedExecutionConfiguration = new ForkedExecutionConfiguration
            {
                BackendAddress = "http://localhost:" + TcpPortHelper.NextFreeTcpPort()
            };

            var host = new BackChannelWebHost(_loggerFactory, new Container(), forkedExecutionConfiguration);

            // Act
            // Assert
            host.Start();

            Assert.Throws<InvalidOperationException>(() => host.Start());
        }
    }
}
