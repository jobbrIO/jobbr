using System;
using System.Collections.Generic;
using System.Linq;

namespace Jobbr.DevSupport.ReferencedVersionAsserter.Tests
{
    public class ExactVersionMatchRule : IAssertionRule
    {
        private readonly string _dependencyName;

        public ExactVersionMatchRule(string dependencyName)
        {
            _dependencyName = dependencyName;
        }

        public bool Validate(List<NuspecDependency> packageDependencies, List<NuspecDependency> nuspecDependencies, out string message)
        {
            var packagDep = packageDependencies.Single(pd => pd.Name.Equals(_dependencyName, StringComparison.InvariantCultureIgnoreCase));
            var nuspecDep = nuspecDependencies.Single(pd => pd.Name.Equals(_dependencyName, StringComparison.InvariantCultureIgnoreCase));

            if (packagDep.MinVersion.Major == nuspecDep.MinVersion.Major && packagDep.MinVersion.Minor == nuspecDep.MinVersion.Minor && packagDep.MinVersion.Bugfix == nuspecDep.MinVersion.Bugfix)
            {
                message = null;
                return true;
            }

            message = $"Version of dependency '{_dependencyName}' in packages (Version: '{packagDep.Version}') does not match the version in NuSpec (Version: '{nuspecDep.Version}')";
            return false;
        }
    }
}