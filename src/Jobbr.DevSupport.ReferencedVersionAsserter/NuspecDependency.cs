namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    public class NuspecDependency
    {
        public string Name { get; set; }

        public string Version { get; set; }

        public NuspecVersion MinVersion { get; set; }

        public NuspecVersion MaxVersion { get; set; }
    }

    public class NuspecVersion
    {
        public int Major { get; set; }

        public int Minor { get; set; }

        public int Bugfix { get; set; }

        public bool Inclusive { get; set; }
    }
}