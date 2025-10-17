using System;
using Jobbr.ComponentModel.JobStorage;
using Jobbr.ComponentModel.Registration;

namespace Jobbr.Storage.LiteDB
{
    public static class JobbrBuilderExtensions
    {
        public static void AddLiteDbStorage(this IJobbrBuilder builder, Action<JobbrLiteDbConfiguration> config)
        {
            var liteDbConfiguration = new JobbrLiteDbConfiguration();

            config(liteDbConfiguration);

            builder.Add<JobbrLiteDbConfiguration>(liteDbConfiguration);

            builder.Register<IJobStorageProvider>(typeof(LiteDbStorageProvider));
            builder.Register<IConfigurationValidator>(typeof(JobbrLiteDbConfigurationValidator));
        }
    }
}