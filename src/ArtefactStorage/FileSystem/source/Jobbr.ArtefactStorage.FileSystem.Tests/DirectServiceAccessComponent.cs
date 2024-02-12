using Jobbr.ComponentModel.ArtefactStorage;
using Jobbr.ComponentModel.Registration;

namespace Jobbr.ArtefactStorage.FileSystem.Tests
{
    public class DirectServiceAccessComponent : IJobbrComponent
    {
        public static DirectServiceAccessComponent Instance;
        public IArtefactsStorageProvider ArtefactStorageProvider { get; }

        public DirectServiceAccessComponent(IArtefactsStorageProvider artefactsStorageProvider)
        {
            Instance = this;
            ArtefactStorageProvider = artefactsStorageProvider;
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
