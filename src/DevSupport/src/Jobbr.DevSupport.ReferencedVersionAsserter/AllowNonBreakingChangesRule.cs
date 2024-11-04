namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    /// <summary>
    /// Rule for allowing non-breaking changes.
    /// </summary>
    public class AllowNonBreakingChangesRule : WildCharNuspecRuleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AllowNonBreakingChangesRule"/> class.
        /// </summary>
        /// <param name="packageName">Name of the validation target package.</param>
        public AllowNonBreakingChangesRule(string packageName) : base(packageName)
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

            if (specMax.Major == specMin.Major && !(specMax.Minor > specMin.Minor))
            {
                message = $"There package reference '{nuspecDependency.Name}' with version '{nuspecDependency.Version}' does not allow non-breaking changes";
                return false;
            }

            message = null;
            return false;
        }
    }
}
