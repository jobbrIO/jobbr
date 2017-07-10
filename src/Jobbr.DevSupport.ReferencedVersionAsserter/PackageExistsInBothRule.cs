using System;
using System.Collections.Generic;
using System.Linq;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    public class PackageExistsInBothRule : IAssertionRule
    {
        private readonly string dependencyName;

        public PackageExistsInBothRule(string dependencyName)
        {
            this.dependencyName = dependencyName;
        }

        public virtual bool Validate(List<NuspecDependency> nuspecDependencies, List<NuspecDependency> packageDependencies, out string message)
        {
            var packagDep = packageDependencies.SingleOrDefault(pd => pd.Name.Equals(this.dependencyName, StringComparison.InvariantCultureIgnoreCase));
            var nuspecDep = nuspecDependencies.SingleOrDefault(pd => pd.Name.Equals(this.dependencyName, StringComparison.InvariantCultureIgnoreCase));

            message = String.Empty;

            if (packagDep == null)
            {
                message = $"Dependency '{this.dependencyName}' is not referenced in packages config.";
            }
            if (nuspecDep == null)
            {
                message = (string.IsNullOrWhiteSpace(message) ? String.Empty : "\n") + $"Dependency '{this.dependencyName}' is not specified in NuGet Package specification.";
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                message = null;
                return true;
            }

            return false;
        }
    }
}