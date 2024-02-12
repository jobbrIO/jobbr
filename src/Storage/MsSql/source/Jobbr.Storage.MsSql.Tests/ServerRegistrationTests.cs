using Jobbr.ComponentModel.Registration;
using Jobbr.Server.Builder;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.Storage.MsSql.Tests
{
    [TestClass]
    public class ServerRegistrationTests
    {
        [TestMethod]
        public void RegisteredAsComponent_JobbrIsStarted_ProviderHasCorrectType()
        {
            var builder = new JobbrBuilder(new NullLoggerFactory());

            builder.RegisterForCollection<IJobbrComponent>(typeof(ExposeStorageProvider));

            builder.AddMsSqlStorage(config =>
            {
                config.ConnectionString = @"Server=.\INSTANCENAME;Integrated Security=true;InitialCatalog=NotUsed;";
            });

            builder.Create();

            Assert.AreEqual(typeof(MsSqlStorageProvider), ExposeStorageProvider.Instance.JobStorageProvider.GetType());
        }

        //[TestMethod]
        //[Ignore]
        //public void RegisteredAsComponent_WithBasicConfiguration_DoesStart()
        //{
        //    var connectionString = GivenDatabaseInstance();
        //    var builder = new JobbrBuilder();
        //    builder.Register<IJobbrComponent>(typeof(ExposeStorageProvider));

        //    builder.AddMsSqlStorage(config =>
        //    {
        //        config.ConnectionString = connectionString;
        //    });

        //    using (var server = builder.Create())
        //    {
        //        server.Start();

        //        Assert.AreEqual(JobbrState.Running, server.State, "Server should be possible to start with default configuration");
        //    }
        //}

        //private static string GivenDatabaseInstance()
        //{
        //    var localDb = new LocalDb();
        //    var sqlConnection = localDb.CreateSqlConnection();
        //    sqlConnection.Open();

        //    var sqlStatements = SqlHelper.SplitSqlStatements(File.ReadAllText("CreateSchemaAndTables.sql")).ToList();

        //    foreach (var statement in sqlStatements)
        //    {
        //        using (var command = sqlConnection.CreateCommand())
        //        {
        //            command.CommandText = statement;
        //            command.ExecuteNonQuery();
        //        }
        //    }

        //    return localDb.ConnectionStringName;
        //}
    }
}