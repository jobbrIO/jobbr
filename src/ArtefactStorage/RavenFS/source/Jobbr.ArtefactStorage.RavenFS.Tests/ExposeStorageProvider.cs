using Jobbr.ComponentModel.ArtefactStorage;
using Jobbr.ComponentModel.Registration;

namespace Jobbr.ArtefactStorage.RavenFS.Tests
{
    public class ExposeArtefactStorageProvider : IJobbrComponent
    {
        internal readonly IArtefactsStorageProvider ArtefactStorageProvider;

        public static ExposeArtefactStorageProvider Instance;

        public ExposeArtefactStorageProvider(IArtefactsStorageProvider artefactStorageProvider)
        {
            this.ArtefactStorageProvider = artefactStorageProvider;
            Instance = this;
        }
        public void Dispose()
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}