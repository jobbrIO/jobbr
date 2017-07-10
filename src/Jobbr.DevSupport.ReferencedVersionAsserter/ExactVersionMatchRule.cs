using System;
using System.Collections.Generic;
using System.Linq;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    public class ExactVersionMatchRule : PackageExistsInBothRule
    {
        private readonly string dependencyName;

        public ExactVersionMatchRule(string dependencyName) : base(dependencyName)
        {
            this.dependencyName = dependencyName;
        }

        public override bool Validate(List<NuspecDependency> nuspecDependencies, List<NuspecDependency> packageDependencies, out string message)
        {
            message = "";

            string searchString;

            if (!this.dependencyName.EndsWith("*"))
            {
                if (!base.Validate(nuspecDependencies, packageDependencies, out message))
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

                if (packagDep.MinVersion.Major == nuspecDep.MinVersion.Major && packagDep.MinVersion.Minor == nuspecDep.MinVersion.Minor && packagDep.MinVersion.Bugfix == nuspecDep.MinVersion.Bugfix && packagDep.MinVersion.Tag == nuspecDep.MinVersion.Tag)
                {
                    continue;
                }

                message = message + (!string.IsNullOrWhiteSpace(message) ? "\n" : "") + $"Version of dependency '{packagDep.Name}' in NuSpec (Version: '{nuspecDep.Version}') does not match the version in packages (Version: '{packagDep.Version}')  ";
            }

            return string.IsNullOrWhiteSpace(message);
        }
    }
}