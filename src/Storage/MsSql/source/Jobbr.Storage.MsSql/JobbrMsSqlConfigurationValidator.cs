using System;
using Jobbr.ComponentModel.Registration;
using Microsoft.Extensions.Logging;

namespace Jobbr.Storage.MsSql
{
    internal class JobbrMsSqlConfigurationValidator : IConfigurationValidator
    {
        private readonly ILogger<MsSqlStorageProvider> _logger;

        public JobbrMsSqlConfigurationValidator(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MsSqlStorageProvider>();
        }

        public Type ConfigurationType { get; set; } = typeof(JobbrMsSqlConfiguration);

        public bool Validate(object configuration)
        {
            if (!(configuration is JobbrMsSqlConfiguration config))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(config.ConnectionString))
            {
                _logger.LogError("No connection string provided.");
                return false;
            }

            if (config.Retention.HasValue &&
                config.Retention.Value < TimeSpan.FromDays(1))
            {
                _logger.LogError("Retention must be bigger than 1 day.");
                return false;
            }

            return true;
        }
    }
}