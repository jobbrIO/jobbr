using System;
using System.Collections.Generic;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    public class VersionIsIncludedInRange : WildCharNuspecRuleBase
    {
        public VersionIsIncludedInRange(string packageName) : base(packageName)
        {
        }

        protected override bool Validate(NuspecDependency nuspecDependencies, NuspecDependency nuConfigPackage, out string message)
        {
            var lowerEnd = nuspecDependencies.MinVersion;
            var lowVersion = new Version(lowerEnd.Major, lowerEnd.Minor, lowerEnd.Bugfix);

            var actualVersion = new Version(nuConfigPackage.MinVersion.Major, nuConfigPackage.MinVersion.Minor, nuConfigPackage.MinVersion.Bugfix);

            bool greateOrEqualLowBound;

            if (lowerEnd.Inclusive)
            {
                greateOrEqualLowBound = actualVersion >= lowVersion;
            }
            else
            {
                greateOrEqualLowBound = actualVersion > lowVersion;
            }

            bool lowerOrEqualUpperBound;

            if (nuspecDependencies.MaxVersion == null)
            {
                lowerOrEqualUpperBound = true;
            }
            else
            {
                var upperEnd = nuspecDependencies.MaxVersion;
                var upVersion = new Version(upperEnd.Major, upperEnd.Minor, upperEnd.Bugfix);

                if (upperEnd.Inclusive)
                {
                    lowerOrEqualUpperBound = actualVersion <= upVersion;
                }
                else
                {
                    lowerOrEqualUpperBound = actualVersion < upVersion;
                }
            }

            if (greateOrEqualLowBound && lowerOrEqualUpperBound)
            {
                message = null;
                return true;
            }

            message = $"Referenced package '{nuConfigPackage.Name}' (Version: {nuConfigPackage.Version} in packages config is not in range of Nuspec version '{nuspecDependencies.Version}'";

            return false;
        }
    }
}