using Jobbr.ComponentModel.JobStorage;
using Jobbr.ComponentModel.Registration;

namespace Jobbr.Storage.LiteDB.Tests
{
    public class ExposeStorageProvider : IJobbrComponent
    {
        public ExposeStorageProvider(IJobStorageProvider jobStorageProvider)
        {
            Instance = this;
            JobStorageProvider = jobStorageProvider;
        }

        public static ExposeStorageProvider Instance { get; private set; }

        internal IJobStorageProvider JobStorageProvider { get; }

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