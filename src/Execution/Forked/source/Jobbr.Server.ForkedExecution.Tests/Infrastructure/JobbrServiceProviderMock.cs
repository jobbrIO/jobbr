using System;
using System.Collections.Generic;
using Jobbr.ComponentModel.Registration;
using Jobbr.Server.ForkedExecution.BackChannel;
using Microsoft.Extensions.Logging;

namespace Jobbr.Server.ForkedExecution.Tests.Infrastructure
{
    public class JobbrServiceProviderMock : IJobbrServiceProvider
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly JobRunInfoServiceMock _jobRunInformationService;
        private readonly ProgressChannelStore _progressChannelStore;

        public JobbrServiceProviderMock(ILoggerFactory loggerFactory, JobRunInfoServiceMock jobRunInformationService, ProgressChannelStore progressChannelStore)
        {
            _loggerFactory = loggerFactory;
            _jobRunInformationService = jobRunInformationService;
            _progressChannelStore = progressChannelStore;
        }

        public object GetService(Type serviceType)
        {
            return serviceType == typeof(ForkedExecutionController) ? new ForkedExecutionController(_loggerFactory, _jobRunInformationService, _progressChannelStore) : null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return new List<object>();
        }

        public IJobbrServiceProvider GetChild()
        {
            return this;
        }
    }
}