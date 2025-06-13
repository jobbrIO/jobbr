using System;

namespace Jobbr.ComponentModel.Registration
{
    public interface IConfigurationValidator
    {
        Type ConfigurationType { get; set; }

        bool Validate(object configuration);
    }
}
