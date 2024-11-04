using System;
using Jobbr.ComponentModel.ArtefactStorage;
using Jobbr.ComponentModel.Registration;

namespace Jobbr.ArtefactStorage.RavenFS
{
    public static class JobbrBuilderExtensions
    {
        public static void AddRavenFsArtefactStorage(this IJobbrBuilder builder, Action<RavenFsConfiguration> config)
        {
            var defaultConfig = new RavenFsConfiguration();

            config(defaultConfig);

            builder.Add<RavenFsConfiguration>(defaultConfig);

            builder.Register<IArtefactsStorageProvider>(typeof(RavenFsArtefactStorageProvider));
            builder.Register<IConfigurationValidator>(typeof(RavenFsConfigurationValidator));
        }
    }
}
