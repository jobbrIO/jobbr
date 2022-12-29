namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    /// <summary>
    /// .nuspec dependency information.
    /// </summary>
    public class NuspecDependency
    {
        /// <summary>
        /// Dependency name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Dependency version.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The lowest allowed dependency version.
        /// </summary>
        public NuspecVersion MinVersion { get; set; }

        /// <summary>
        /// The highest allowed dependency version.
        /// </summary>
        public NuspecVersion MaxVersion { get; set; }
    }
}