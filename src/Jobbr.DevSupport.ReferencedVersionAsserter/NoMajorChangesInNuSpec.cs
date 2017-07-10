namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    public class NoMajorChangesInNuSpec : WildCharNuspecRuleBase
    {
        public NoMajorChangesInNuSpec(string packageName) : base(packageName)
        {
        }

        protected override bool Validate(NuspecDependency nuspecDependencies, NuspecDependency packageDependency, out string message)
        {
            // No Max => major updates are allowed
            if (nuspecDependencies.MaxVersion == null)
            {
                message = $"Package '{nuspecDependencies.Name}' does not specify a upper version, which means every major update is allowed.";
                return false;
            }

            // Version Range with inclusive max
            if (nuspecDependencies.MaxVersion.Major > nuspecDependencies.MinVersion.Major && nuspecDependencies.MaxVersion.Inclusive)
            {
                message = $"Package dependency '{nuspecDependencies.Name}' allows major updates.";
                return false;
            }

            message = null;
            return true;
        }

    }
}