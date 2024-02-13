namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    /// <summary>
    /// Rule for disallowing major version changes.
    /// </summary>
    public class NoMajorChangesInNuSpec : WildCharNuspecRuleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoMajorChangesInNuSpec"/> class.
        /// </summary>
        /// <param name="packageName">Target package name.</param>
        public NoMajorChangesInNuSpec(string packageName) : base(packageName)
        {
        }

        /// <inheritdoc/>
        protected override bool Validate(NuspecDependency nuspecDependency, NuspecDependency packageDependency, out string message)
        {
            // No Max => major updates are allowed
            if (nuspecDependency.MaxVersion == null)
            {
                message = $"Package '{nuspecDependency.Name}' does not specify a upper version, which means every major update is allowed.";
                return false;
            }

            // Version Range with inclusive max
            if (nuspecDependency.MaxVersion.Major > nuspecDependency.MinVersion.Major && nuspecDependency.MaxVersion.Inclusive)
            {
                message = $"Package dependency '{nuspecDependency.Name}' allows major updates.";
                return false;
            }

            message = null;
            return true;
        }
    }
}