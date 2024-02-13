using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Jobbr.ComponentModel.Execution;
using Jobbr.ComponentModel.Execution.Model;
using Jobbr.Server.ForkedExecution.BackChannel;
using Jobbr.Server.ForkedExecution.Tests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector;

namespace Jobbr.Server.ForkedExecution.Tests
{
    [TestClass]
    public class EndpointTests
    {
        private readonly string _configBackendAddress;
        private readonly FakeGeneratedJobRunsStore _fakeStore = new ();
        private readonly ProgressChannelStore _channelFakeStore = new ();

        public EndpointTests()
        {
            _configBackendAddress = $"http://localhost:{TcpPortHelper.NextFreeTcpPort()}";
        }

        [TestMethod]
        public void JobInfoEndpoint_GetNonExistingById_ReturnsNotFound()
        {
            // Arrange
            GivenARunningServer();

            // Act
            var statusResponse = Get("/fex/jobrun/524868244");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, statusResponse.StatusCode);
        }

        [TestMethod]
        public void JobInfoEndpoint_GetExistingById_ReturnsOk()
        {
            // Arrange
            GivenARunningServer();
            var createdJobRun = _fakeStore.CreateFakeJobRun(DateTime.UtcNow);

            // Act
            var statusResponse = Get("/fex/jobrun/" + createdJobRun.Id);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, statusResponse.StatusCode);
        }

        [TestMethod]
        public void JobRunEndpoint_UpdateExisting_ReturnsAccepted()
        {
            // Arrange
            GivenARunningServer();
            var createdJobRun = _fakeStore.CreateFakeJobRun(DateTime.UtcNow);

            // Act
            var statusResponse = Put("/fex/jobrun/" + createdJobRun.Id, new { State = JobRunStates.Started });

            // Assert
            Assert.AreEqual(HttpStatusCode.Accepted, statusResponse.StatusCode);
            Assert.AreEqual(1, _channelFakeStore.AllStatusUpdates.SelectMany(u => u.Value).Count(), "There should be one single update");
        }

        [TestMethod]
        public void JobRunEndpoint_UpdateNotExisting_ReturnsNotFound()
        {
            // Arrange
            GivenARunningServer();

            // Act
            var statusResponse = Put("/fex/jobrun/524868244", new { });

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, statusResponse.StatusCode);
        }

        [TestMethod]
        public void JobRunEndpoint_SendInvalidState_ReturnsNotFound()
        {
            // Arrange
            GivenARunningServer();
            var createdJobRun = _fakeStore.CreateFakeJobRun(DateTime.UtcNow);

            // Act
            var statusResponse = Put("/fex/jobrun/" + createdJobRun.Id, new { State = "MyCustomState" });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, statusResponse.StatusCode);
        }

        [TestMethod]
        public void ArtefactEndpoint_SendForKnownJobRun_ReturnsAccepted()
        {
            // Arrange
            GivenARunningServer();
            var createdJobRun = _fakeStore.CreateFakeJobRun(DateTime.UtcNow);

            // Act
            var statusResponse = SendImage("/fex/jobrun/" + createdJobRun.Id + "/artefacts", new MemoryStream());

            // Assert
            Assert.AreEqual(HttpStatusCode.Accepted, statusResponse.StatusCode);
            Assert.AreEqual(1, _channelFakeStore.AllUploadedArtefacts.SelectMany(a => a.Value).Count());
        }

        [TestMethod]
        public void ArtefactEndpoint_SendForUnknownJobRun_ReturnsNotFound()
        {
            // Arrange
            GivenARunningServer();

            // Act
            var statusResponse = SendImage("/fex/jobrun/524868244/artefacts", new MemoryStream());

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, statusResponse.StatusCode);
        }

        private HttpResponseMessage Get(string url)
        {
            return new HttpClient().GetAsync(_configBackendAddress + url).Result;
        }

        private HttpResponseMessage Put(string url, object json)
        {
            return new HttpClient().PutAsJsonAsync(_configBackendAddress + url, json).Result;
        }

        private HttpResponseMessage SendImage(string url, MemoryStream fileStream)
        {
            var requestContent = new MultipartFormDataContent(); // here you can specify boundary if you need
            var imageContent = new ByteArrayContent(fileStream.ToArray());
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

            requestContent.Add(imageContent, "image", "image.jpg");

            return new HttpClient().PostAsync(_configBackendAddress + url, requestContent).Result;
        }

        private void GivenARunningServer()
        {
            var config = new ForkedExecutionConfiguration
            {
                BackendAddress = _configBackendAddress
            };

            var serviceCollection = new Container();
            serviceCollection.RegisterInstance<IJobRunInformationService>(new JobRunInfoServiceMock(_fakeStore));
            serviceCollection.RegisterInstance<IJobRunProgressChannel>(_channelFakeStore);

            var webHost = new BackChannelWebHost(NullLoggerFactory.Instance, serviceCollection, config);

            webHost.Start();
        }
    }
}
