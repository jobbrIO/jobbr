using System;
using Jobbr.ComponentModel.Registration;

namespace Jobbr.ArtefactStorage.RavenFS
{
    public class RavenFsConfigurationValidator : IConfigurationValidator
    {
        public Type ConfigurationType { get; set; } = typeof(RavenFsConfiguration);

        public bool Validate(object toValidate)
        {
            var configuration = (RavenFsConfiguration) toValidate;

            if (string.IsNullOrWhiteSpace(configuration.Url))
            {
                throw new InvalidOperationException("Url must be set for RavenFS");
            }

            if (string.IsNullOrWhiteSpace(configuration.FileSystem))
            {
                throw new InvalidOperationException("FileSystem must be set for RavenFS");
            }

            return true;
        }
    }
}