using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Jobbr.ComponentModel.Registration;
using Microsoft.Extensions.Logging;

namespace Jobbr.Server.ForkedExecution
{
    /// <summary>
    /// Validates configuration.
    /// </summary>
    public class ConfigurationValidator : IConfigurationValidator
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationValidator"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        public ConfigurationValidator(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ConfigurationValidator>();
        }

        /// <summary>
        /// Configuration type.
        /// </summary>
        public Type ConfigurationType { get; set; } = typeof(ForkedExecutionConfiguration);

        /// <summary>
        /// Validate configuration.
        /// </summary>
        /// <param name="configuration">Configuration.</param>
        /// <returns>If configuration was valid.</returns>
        /// <exception cref="ArgumentException">Issue in the configuration.</exception>
        public bool Validate(object configuration)
        {
            var config = (ForkedExecutionConfiguration)configuration;

            if (config == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(config.BackendAddress))
            {
                // Fallback to automatic endpoint port
                _logger.LogWarning("There was no backend address specified. Falling back to random port, which is not guaranteed to work in production scenarios.");
                var port = NextFreeTcpPort();

                config.BackendAddress = $"http://localhost:{port}";
            }

            if (string.IsNullOrEmpty(config.JobRunDirectory))
            {
                throw new ArgumentException("Please provide a JobRunDirectory!");
            }

            var jobRunDirectoryPath = Path.GetFullPath(config.JobRunDirectory);
            if (!Directory.Exists(jobRunDirectoryPath))
            {
                throw new ArgumentException($"The Directory '{jobRunDirectoryPath}' cannot be found!");
            }

            if (string.IsNullOrWhiteSpace(config.JobRunnerExecutable))
            {
                throw new ArgumentException("You should set a runner-Executable which runs your jobs later!");
            }

            var executableFullPath = Path.GetFullPath(config.JobRunnerExecutable);

            if (!File.Exists(executableFullPath))
            {
                throw new ArgumentException($"The RunnerExecutable '{executableFullPath}' cannot be found!");
            }

            return true;
        }

        private static int NextFreeTcpPort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            var port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}
