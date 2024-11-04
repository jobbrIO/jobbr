using System;
using System.Collections.Generic;
using System.Linq;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    /// <summary>
    /// Rule for matching the exact version.
    /// </summary>
    public class ExactVersionMatchRule : PackageExistsInBothRule
    {
        private readonly string _dependencyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExactVersionMatchRule"/> class.
        /// </summary>
        /// <param name="dependencyName">The name of the target dependency.</param>
        public ExactVersionMatchRule(string dependencyName) : base(dependencyName)
        {
            _dependencyName = dependencyName;
        }

        /// <inheritdoc/>
        public override bool Validate(List<NuspecDependency> nuspecDependencies, List<NuspecDependency> packageDependencies, out string message)
        {
            message = string.Empty;

            string searchString;

            if (!_dependencyName.EndsWith("*"))
            {
                if (!base.Validate(nuspecDependencies, packageDependencies, out message))
                {
                    return false;
                }

                searchString = _dependencyName;
            }
            else
            {
                searchString = _dependencyName.Substring(0, _dependencyName.Length - 1);
            }

            var foundPackageDeps = packageDependencies.Where(pd => pd.Name.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase));
            var foundNuSpecDependencies = nuspecDependencies.Where(pd => pd.Name.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase));

            var existingInBoth = foundPackageDeps.Join(foundNuSpecDependencies, left => left.Name, right => right.Name, (left, right) => new Tuple<NuspecDependency, NuspecDependency>(left, right));

            foreach (var (packageDep, nuspecDep) in existingInBoth)
            {
                if (packageDep.MinVersion.Major == nuspecDep.MinVersion.Major
                    && packageDep.MinVersion.Minor == nuspecDep.MinVersion.Minor
                    && packageDep.MinVersion.Bugfix == nuspecDep.MinVersion.Bugfix
                    && packageDep.MinVersion.Tag == nuspecDep.MinVersion.Tag)
                {
                    continue;
                }

                message = message + (!string.IsNullOrWhiteSpace(message) ? "\n" : string.Empty) + $"Version of dependency '{packageDep.Name}' in NuSpec (Version: '{nuspecDep.Version}') does not match the version in packages (Version: '{packageDep.Version}')";
            }

            return string.IsNullOrWhiteSpace(message);
        }
    }
}