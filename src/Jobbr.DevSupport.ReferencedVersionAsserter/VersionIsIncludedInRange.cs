using System;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    public class VersionIsIncludedInRange : WildCharNuspecRuleBase
    {
        public VersionIsIncludedInRange(string packageName) : base(packageName)
        {
        }

        protected override bool Validate(NuspecDependency nuspecDependency, NuspecDependency nuConfigPackage, out string message)
        {
            var lowerEnd = nuspecDependency.MinVersion;
            var lowVersion = new Version(lowerEnd.Major, lowerEnd.Minor, lowerEnd.Bugfix);

            var actualVersion = new Version(nuConfigPackage.MinVersion.Major, nuConfigPackage.MinVersion.Minor, nuConfigPackage.MinVersion.Bugfix);

            bool greaterOrEqualLowBound;

            if (lowerEnd.Inclusive)
            {
                greaterOrEqualLowBound = actualVersion >= lowVersion;
            }
            else
            {
                greaterOrEqualLowBound = actualVersion > lowVersion;
            }

            bool lowerOrEqualUpperBound;

            if (nuspecDependency.MaxVersion == null)
            {
                lowerOrEqualUpperBound = true;
            }
            else
            {
                var upperEnd = nuspecDependency.MaxVersion;
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

            if (greaterOrEqualLowBound && lowerOrEqualUpperBound)
            {
                message = null;
                return true;
            }

            message = $"Referenced package '{nuConfigPackage.Name}' (Version: {nuConfigPackage.Version} in packages config is not in range of Nuspec version '{nuspecDependency.Version}'";

            return false;
        }
    }
}