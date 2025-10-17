using Jobbr.ComponentModel.Registration;
using Jobbr.Server.Builder;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.Storage.LiteDB.Tests
{
    [TestClass]
    public class ServerRegistrationTests
    {
        [TestMethod]
        public void RegisteredAsComponent_JobbrIsStarted_ProviderHasCorrectType()
        {
            var builder = new JobbrBuilder(new NullLoggerFactory());

            builder.RegisterForCollection<IJobbrComponent>(typeof(ExposeStorageProvider));

            builder.AddLiteDbStorage(config =>
            {
                config.ConnectionString = ":memory:";
            });

            builder.Create();

            Assert.AreEqual(typeof(LiteDbStorageProvider), ExposeStorageProvider.Instance.JobStorageProvider.GetType());
        }
    }
}