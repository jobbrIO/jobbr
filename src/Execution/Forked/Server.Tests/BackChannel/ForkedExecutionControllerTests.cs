using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Jobbr.ComponentModel.Execution;
using Jobbr.ComponentModel.Execution.Model;
using Jobbr.Server.ForkedExecution.BackChannel;
using Jobbr.Server.ForkedExecution.BackChannel.Model;
using Jobbr.Server.ForkedExecution.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Jobbr.Server.ForkedExecution.Tests.BackChannel
{
    [TestClass]
    public class ForkedExecutionControllerTests
    {
        private readonly FakeGeneratedJobRunsStore _fakeStore;
        private readonly ForkedExecutionController _controller;

        private readonly Mock<IJobRunInformationService> _jobRunInfoServiceMock;
        private readonly Mock<IJobRunProgressChannel> _progressChannelStoreMock;

        public ForkedExecutionControllerTests()
        {
            _fakeStore = new FakeGeneratedJobRunsStore();

            _progressChannelStoreMock = new Mock<IJobRunProgressChannel>();
            _jobRunInfoServiceMock = new Mock<IJobRunInformationService>();

            _controller = new ForkedExecutionController(NullLoggerFactory.Instance, _jobRunInfoServiceMock.Object, _progressChannelStoreMock.Object);
        }

        [TestMethod]
        public void JobInfoEndpoint_GetExistingById_ReturnsOk()
        {
            // Arrange
            var createdJobRun = _fakeStore.CreateFakeJobRun(DateTime.UtcNow);
            _jobRunInfoServiceMock.Setup(m => m.GetByJobRunId(createdJobRun.Id)).Returns(createdJobRun.JobRunInfo);

            // Act
            var statusResponse = _controller.GetJobRunInfos(createdJobRun.Id);

            // Assert
            Assert.IsInstanceOfType(statusResponse, typeof(OkObjectResult));
            _jobRunInfoServiceMock.Verify(m => m.GetByJobRunId(createdJobRun.Id), Times.Once);
        }

        [TestMethod]
        public void JobInfoEndpoint_GetNonExistingById_ReturnsNotFound()
        {
            // Arrange
            const int nonExistingJobRunId = 2491242;
            _jobRunInfoServiceMock.Setup(m => m.GetByJobRunId(nonExistingJobRunId)).Returns((JobRunInfo)null);

            // Act
            var statusResponse = _controller.GetJobRunInfos(nonExistingJobRunId);

            // Assert
            Assert.IsInstanceOfType(statusResponse, typeof(NotFoundResult));
            _jobRunInfoServiceMock.Verify(m => m.GetByJobRunId(nonExistingJobRunId), Times.Once);
        }

        [TestMethod]
        public void JobRunEndpoint_UpdateExisting_ReturnsAccepted()
        {
            // Arrange
            var createdJobRun = _fakeStore.CreateFakeJobRun(DateTime.UtcNow);
            _jobRunInfoServiceMock.Setup(m => m.GetByJobRunId(createdJobRun.Id)).Returns(createdJobRun.JobRunInfo);
            _progressChannelStoreMock.Setup(m => m.PublishStatusUpdate(createdJobRun.Id, JobRunStates.Started));

            // Act
            var statusResponse = _controller.PutJobRunUpdate(createdJobRun.Id, new JobRunUpdateDto { State = JobRunStates.Started });

            // Assert
            Assert.IsInstanceOfType(statusResponse, typeof(AcceptedResult));

            _jobRunInfoServiceMock.Verify(m => m.GetByJobRunId(createdJobRun.Id), Times.Once);
            _progressChannelStoreMock.Verify(m => m.PublishStatusUpdate(createdJobRun.Id, JobRunStates.Started), Times.Once);
        }

        [TestMethod]
        public void JobRunEndpoint_UpdateNotExisting_ReturnsNotFound()
        {
            // Arrange
            const int nonExistingJobRunId = 2491242;
            _jobRunInfoServiceMock.Setup(m => m.GetByJobRunId(nonExistingJobRunId)).Returns((JobRunInfo)null);
            _progressChannelStoreMock.Setup(m => m.PublishStatusUpdate(It.IsAny<long>(), It.IsAny<JobRunStates>()));

            // Act
            var statusResponse = _controller.PutJobRunUpdate(nonExistingJobRunId, new JobRunUpdateDto { State = JobRunStates.Started });

            // Assert
            Assert.IsInstanceOfType(statusResponse, typeof(NotFoundResult));

            _jobRunInfoServiceMock.Verify(m => m.GetByJobRunId(nonExistingJobRunId), Times.Once);
            _progressChannelStoreMock.Verify(m => m.PublishStatusUpdate(It.IsAny<long>(), It.IsAny<JobRunStates>()), Times.Never);
        }

        [TestMethod]
        public void ArtefactEndpoint_SendForKnownJobRun_ReturnsAccepted()
        {
            // Arrange
            var createdJobRun = _fakeStore.CreateFakeJobRun(DateTime.UtcNow);
            _jobRunInfoServiceMock.Setup(m => m.GetByJobRunId(createdJobRun.Id)).Returns(createdJobRun.JobRunInfo);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Content-Type"] = "multipart/form-data; boundary=--THIS_STRING_SEPARATES";
            const string fileName = "dummy.txt";
            var file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 1000, "Data", fileName);
            httpContext.Request.Form = new FormCollection(new Dictionary<string, StringValues>(), new FormFileCollection { file });
            var actx = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());

            _controller.ControllerContext = new ControllerContext(actx);

            // Act
            var statusResponse = _controller.AddArtefacts(createdJobRun.Id);

            // Assert
            Assert.IsInstanceOfType(statusResponse, typeof(AcceptedResult));

            _jobRunInfoServiceMock.Verify(m => m.GetByJobRunId(createdJobRun.Id), Times.Once);
            _progressChannelStoreMock.Verify(m => m.PublishArtefact(createdJobRun.Id, fileName, It.IsAny<Stream>()), Times.Once);
        }

        [TestMethod]
        public void ArtefactEndpoint_SendForUnknownJobRun_ReturnsNotFound()
        {
            // Arrange
            const int nonExistingJobRunId = 2491242;
            _jobRunInfoServiceMock.Setup(m => m.GetByJobRunId(nonExistingJobRunId)).Returns((JobRunInfo)null);
            _progressChannelStoreMock.Setup(m => m.PublishArtefact(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<Stream>()));

            // Act
            var statusResponse = _controller.AddArtefacts(nonExistingJobRunId); // TODO fix fileupload endpoint

            // Assert
            Assert.IsInstanceOfType(statusResponse, typeof(NotFoundResult));

            _jobRunInfoServiceMock.Verify(m => m.GetByJobRunId(nonExistingJobRunId), Times.Once);
            _progressChannelStoreMock.Verify(m => m.PublishArtefact(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<Stream>()), Times.Never);
        }
    }
}
