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

        public virtual bool Validate(List<NuspecDependency> packageDependencies, List<NuspecDependency> nuspecDependencies, out string message)
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

    public class ExactVersionMatchRule : PackageExistsInBothRule
    {
        private readonly string dependencyName;

        public ExactVersionMatchRule(string dependencyName) : base(dependencyName)
        {
            this.dependencyName = dependencyName;
        }

        public override bool Validate(List<NuspecDependency> packageDependencies, List<NuspecDependency> nuspecDependencies, out string message)
        {
            message = "";

            string searchString;

            if (!this.dependencyName.EndsWith("*"))
            {
                if (!base.Validate(packageDependencies, nuspecDependencies, out message))
                {
                    return false;
                }

                searchString = this.dependencyName;
            }
            else 
            {
                searchString = this.dependencyName.Substring(0, this.dependencyName.Length - 1);
            }

            var foundPackageDeps = packageDependencies.Where(pd => pd.Name.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase));
            var foundNuSpecDependencies = nuspecDependencies.Where(pd => pd.Name.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase));

            var existingInBoth = foundPackageDeps.Join(foundNuSpecDependencies, left => left.Name, right => right.Name, (left, right) => new Tuple<NuspecDependency, NuspecDependency>(left, right));

            foreach (var dependency in existingInBoth)
            {
                var packagDep = dependency.Item1;
                var nuspecDep = dependency.Item2;

                if (packagDep.MinVersion.Major == nuspecDep.MinVersion.Major && packagDep.MinVersion.Minor == nuspecDep.MinVersion.Minor && packagDep.MinVersion.Bugfix == nuspecDep.MinVersion.Bugfix)
                {
                    continue;
                }

                message = message + (string.IsNullOrWhiteSpace(message) ? "\n" : "") + $"Version of dependency '{this.dependencyName}' in packages (Version: '{packagDep.Version}') does not match the version in NuSpec (Version: '{nuspecDep.Version}')";
            }

            return string.IsNullOrWhiteSpace(message);
        }
    }
}