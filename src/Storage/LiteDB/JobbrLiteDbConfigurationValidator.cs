using System;
using Jobbr.ComponentModel.Registration;

namespace Jobbr.Storage.LiteDB
{
    public class JobbrLiteDbConfigurationValidator : IConfigurationValidator
    {
        public Type ConfigurationType { get; set; } = typeof(JobbrLiteDbConfiguration);

        public bool Validate(object configuration)
        {
            if (configuration is not JobbrLiteDbConfiguration config)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(config.ConnectionString))
            {
                return false;
            }

            return true;
        }
    }
}