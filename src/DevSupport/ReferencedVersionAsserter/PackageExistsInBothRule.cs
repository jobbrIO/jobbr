using System;
using System.Collections.Generic;
using System.Linq;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    /// <summary>
    /// Rule that makes sure that package exists in both configurations.
    /// </summary>
    public class PackageExistsInBothRule : IAssertionRule
    {
        private readonly string _dependencyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageExistsInBothRule"/> class.
        /// </summary>
        /// <param name="dependencyName">Target dependency name.</param>
        public PackageExistsInBothRule(string dependencyName)
        {
            _dependencyName = dependencyName;
        }

        /// <inheritdoc/>
        public virtual bool Validate(List<NuspecDependency> nuspecDependencies, List<NuspecDependency> packageDependencies, out string message)
        {
            var packageDep = packageDependencies.SingleOrDefault(pd => pd.Name.Equals(_dependencyName, StringComparison.InvariantCultureIgnoreCase));
            var nuspecDep = nuspecDependencies.SingleOrDefault(pd => pd.Name.Equals(_dependencyName, StringComparison.InvariantCultureIgnoreCase));

            message = string.Empty;

            if (packageDep == null)
            {
                message = $"Dependency '{_dependencyName}' is not referenced in packages config.";
            }

            if (nuspecDep == null)
            {
                message = message + (string.IsNullOrWhiteSpace(message) ? string.Empty : "\n") + $"Dependency '{_dependencyName}' is not specified in NuGet Package specification.";
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