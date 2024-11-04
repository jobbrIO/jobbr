using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jobbr.ComponentModel.Registration
{
    public interface IConfigurationValidator
    {
        Type ConfigurationType { get; set; }

        bool Validate(object configuration);
    }
}
