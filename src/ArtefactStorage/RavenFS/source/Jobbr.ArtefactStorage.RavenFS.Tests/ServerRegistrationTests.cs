using Jobbr.ComponentModel.Registration;
using Jobbr.Server;
using Jobbr.Server.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.ArtefactStorage.RavenFS.Tests
{
    [TestClass]
    public class ServerRegistrationTests : IntegrationTestBase
    {
        [TestMethod]
        public void RegisteredAsComponent_JobbrIsStarted_ProviderHasCorrectType()
        {
            GivenRavenFs();
            var builder = new JobbrBuilder();
            builder.Register<IJobbrComponent>(typeof(ExposeArtefactStorageProvider));

            builder.AddRavenFsArtefactStorage(config =>
            {
                config.FileSystem = Store.DefaultDatabase;
                config.Url = Store.Url;
            });

            builder.Create();

            Assert.AreEqual(typeof(RavenFsArtefactStorageProvider), ExposeArtefactStorageProvider.Instance.ArtefactStorageProvider.GetType());
        }

        [TestMethod]
        public void RegisteredAsComponent_WithBasicConfiguration_ServerDoesStart()
        {
            GivenRavenFs();

            var builder = new JobbrBuilder();
            builder.Register<IJobbrComponent>(typeof(ExposeArtefactStorageProvider));

            builder.AddRavenFsArtefactStorage(config =>
            {
                config.FileSystem = Store.DefaultDatabase;
                config.Url = Store.Url;
            });


            using (var server = builder.Create())
            {
                server.Start();

                Assert.AreEqual(JobbrState.Running, server.State, "Server should be possible to start with a proper configuration.");
            }
        }
    }
}
