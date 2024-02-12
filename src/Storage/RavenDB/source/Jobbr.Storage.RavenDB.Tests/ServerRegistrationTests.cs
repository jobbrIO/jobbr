using Jobbr.ComponentModel.Registration;
using Jobbr.Server;
using Jobbr.Server.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.Storage.RavenDB.Tests
{
    [TestClass]
    public class ServerRegistrationTests : RavenDbIntegrationTestBase
    {
        [TestMethod]
        public void RegisteredAsComponent_JobbrIsStarted_ProviderHasCorrectType()
        {
            GivenRavenDb();
            var builder = new JobbrBuilder();
            builder.Register<IJobbrComponent>(typeof(ExposeStorageProvider));

            builder.AddRavenDbStorage(Store);

            builder.Create();

            Assert.AreEqual(typeof(RavenDbStorageProvider), ExposeStorageProvider.Instance.JobStorageProvider.GetType());
        }

        [TestMethod]
        public void RegisteredAsComponent_WithBasicConfiguration_DoesStart()
        {
            GivenRavenDb();

            var builder = new JobbrBuilder();
            builder.Register<IJobbrComponent>(typeof(ExposeStorageProvider));

            builder.AddRavenDbStorage(Store);

            using (var server = builder.Create())
            {
                server.Start();

                Assert.AreEqual(JobbrState.Running, server.State, "Server should be possible to start with a proper configuration.");
            }
        }
    }
}
