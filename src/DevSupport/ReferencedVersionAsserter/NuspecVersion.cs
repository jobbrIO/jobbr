namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    /// <summary>
    /// .nuspec dependency version information.
    /// </summary>
    public class NuspecVersion
    {
        /// <summary>
        /// Major version.
        /// </summary>
        public int Major { get; set; }

        /// <summary>
        /// Minor version.
        /// </summary>
        public int Minor { get; set; }

        /// <summary>
        /// Bugfix version.
        /// </summary>
        public int Bugfix { get; set; }

        /// <summary>
        /// If the version is inclusive. True if inclusive, otherwise false and exclusive.
        /// </summary>
        public bool Inclusive { get; set; }

        /// <summary>
        /// Tag.
        /// </summary>
        public string Tag { get; set; }
    }
}