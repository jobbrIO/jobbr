namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    /// <summary>
    /// Rule for making sure that dependencies to component models only allow bugfix releases that are considered as MSIL compliant.
    /// </summary>
    public class OnlyAllowBugfixesRule : WildCharNuspecRuleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnlyAllowBugfixesRule"/> class.
        /// </summary>
        /// <param name="packageName">Target package name.</param>
        public OnlyAllowBugfixesRule(string packageName) : base(packageName)
        {
        }

        /// <inheritdoc/>
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