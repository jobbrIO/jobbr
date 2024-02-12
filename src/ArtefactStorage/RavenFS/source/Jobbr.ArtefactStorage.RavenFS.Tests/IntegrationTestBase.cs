using Raven.Client.Document;
using Raven.Database.Config;
using Raven.Tests.Helpers;

namespace Jobbr.ArtefactStorage.RavenFS.Tests
{
    public abstract class IntegrationTestBase : RavenTestBase
    {
        protected DocumentStore Store;
        protected RavenFsConfiguration RavenFsConfiguration;

        protected void GivenRavenFs()
        {
            Store = NewRemoteDocumentStore(requestedStorage: "esent");
            
            Store.Initialize();
        }

        protected void GivenRavenFsConfiguration()
        {
            RavenFsConfiguration = new RavenFsConfiguration
            {
                Url = Store.Url,
                FileSystem = Store.DefaultDatabase
            };
        }

        protected override void ModifyConfiguration(InMemoryRavenConfiguration configuration)
        {
            configuration.Storage.Voron.AllowOn32Bits = true;

            base.ModifyConfiguration(configuration);
        }
    }
}