namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    public class OnlyAllowBugfixesRule : WildCharNuspecRuleBase
    {
        /// <summary>
        /// This rule makes sure that dependencies to Component models only allow BugFix releases that are considered as MSIL compilant
        /// </summary>
        /// <param name="packageName"></param>
        public OnlyAllowBugfixesRule(string packageName) : base (packageName)
        {
        }

        protected override bool Validate(NuspecDependency nuspecDependency, NuspecDependency nuConfigPackage, out string message)
        {
            var specMin = nuspecDependency.MinVersion;
            var specMax = nuspecDependency.MaxVersion;

            if (specMax == null)
            {
                message = $"Cannot evaluate rule for '{nuspecDependency.Name}' if no max range is set";
                return false;
            }

            if (specMin.Major != specMax.Major || (specMin.Major == specMax.Major && specMin.Minor != specMax.Minor && specMax.Inclusive))
            {
                message = $"Package dependency '{nuspecDependency.Name}' more than bug-fix dependencies ({nuspecDependency.Version})!";
                return false;
            }

            message = string.Empty;
            return true;
        }
    }
}