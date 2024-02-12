using Jobbr.ComponentModel.Registration;

namespace Jobbr.ArtefactStorage.RavenFS
{
    public class RavenFsConfiguration : IFeatureConfiguration
    {
        public string Url { get; set; }

        public string FileSystem { get; set; }
    }
}