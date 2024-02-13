using System;
using System.Threading.Tasks;
using Jobbr.ComponentModel.Registration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleInjector;
using Container = SimpleInjector.Container;

namespace Jobbr.Server.ForkedExecution.BackChannel
{
    /// <summary>
    /// Creates and starts BackChannel web host.
    /// </summary>
    public class BackChannelWebHost : IJobbrComponent
    {
        private readonly ILogger _logger;
        private readonly InstanceProducer[] _serviceCollection;
        private readonly ForkedExecutionConfiguration _configuration;
        private WebApplication _webApp;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackChannelWebHost"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="container">The service injector container.</param>
        /// <param name="configuration">Forked execution configuration.</param>
        public BackChannelWebHost(ILoggerFactory loggerFactory, Container container, ForkedExecutionConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<BackChannelWebHost>();
            _logger.LogDebug("Constructing new BackChannelWebHost");

            _serviceCollection = container.GetCurrentRegistrations();
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Start the web host.
        /// </summary>
        /// <exception cref="InvalidOperationException">The host is already running.</exception>
        public void Start()
        {
            if (_webApp != null)
            {
                throw new InvalidOperationException("The server has already been started.");
            }

            var builder = WebApplication.CreateBuilder();

            foreach (var instanceProducer in _serviceCollection)
            {
                builder.Services.Add(new ServiceDescriptor(instanceProducer.ServiceType, instanceProducer.GetInstance()));
            }

            // Controllers with endpoints need to be added manually due discovery issues.
            // https://stackoverflow.com/q/73777145
            var mvcBuilder = builder.Services.AddControllers();
            mvcBuilder.AddApplicationPart(typeof(DefaultController).Assembly);
            mvcBuilder.AddApplicationPart(typeof(ForkedExecutionController).Assembly);

            _webApp = builder.Build();
            _webApp.MapControllers();
            _webApp.Urls.Add(_configuration.BackendAddress);

            Task.FromResult(_webApp.StartAsync());

            _logger.LogInformation("Started web host for Backchannel at '{backendAddress}'.", _configuration.BackendAddress);
        }

        /// <summary>
        /// Stop the hosting by disposing the host object.
        /// </summary>
        public void Stop()
        {
            Task.FromResult(_webApp.StopAsync());
        }

        /// <summary>
        /// Conditional web host dispose.
        /// </summary>
        /// <param name="disposing">If true, dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Task.FromResult(_webApp.StopAsync());
                Task.FromResult(_webApp.DisposeAsync());
            }
        }
    }
}
