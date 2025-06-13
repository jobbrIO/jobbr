using Jobbr.ComponentModel.ArtefactStorage;
using Jobbr.ComponentModel.Registration;

namespace Jobbr.ArtefactStorage.FileSystem.Tests
{
    public class DirectServiceAccessComponent : IJobbrComponent
    {
        public DirectServiceAccessComponent(IArtefactsStorageProvider artefactsStorageProvider)
        {
            Instance = this;
            ArtefactStorageProvider = artefactsStorageProvider;
        }

        public static DirectServiceAccessComponent Instance { get; private set; }

        public IArtefactsStorageProvider ArtefactStorageProvider { get; }

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
