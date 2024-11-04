using Jobbr.ComponentModel.JobStorage;
using Jobbr.ComponentModel.Registration;
using Raven.Client;

namespace Jobbr.Storage.RavenDB
{
    public static class JobbrBuilderExtensions
    {
        public static void AddRavenDbStorage(this IJobbrBuilder builder, IDocumentStore store)
        {           
            builder.Add<IDocumentStore>(store);
            builder.Register<IJobStorageProvider>(typeof(RavenDbStorageProvider));
        }
    }
}
