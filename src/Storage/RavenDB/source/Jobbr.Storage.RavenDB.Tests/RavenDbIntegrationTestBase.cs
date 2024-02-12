using Raven.Client;
using Raven.Database.Config;
using Raven.Tests.Helpers;

namespace Jobbr.Storage.RavenDB.Tests
{
    public abstract class RavenDbIntegrationTestBase : RavenTestBase
    {
        protected IDocumentStore Store;
        protected RavenDbStorageProvider StorageProvider;

        protected void GivenRavenDb()
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            this.Store = this.NewDocumentStore();
        }

        protected override void ModifyConfiguration(InMemoryRavenConfiguration configuration)
        {
            base.ModifyConfiguration(configuration);

            configuration.Storage.Voron.AllowOn32Bits = true;
        }

        protected void GivenStorageProvider()
        {
            this.StorageProvider = new RavenDbStorageProvider(Store);
        }
    }
}