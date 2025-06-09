using System;
using Jobbr.ComponentModel.ArtefactStorage;
using Jobbr.ComponentModel.Registration;

namespace Jobbr.ArtefactStorage.FileSystem
{
    public static class JobbrBuilderExtensions
    {
        public static void AddFileSystemArtefactStorage(this IJobbrBuilder builder, Action<FileSystemArtefactStorageConfiguration> config)
        {
            var defaultConfig = new FileSystemArtefactStorageConfiguration();

            config(defaultConfig);

            builder.Add<FileSystemArtefactStorageConfiguration>(defaultConfig);

            builder.Register<IArtefactsStorageProvider>(typeof(FileSystemArtefactsStorageProvider));
            builder.RegisterForCollection<IConfigurationValidator>(typeof(ConfigurationValidator));
        }
    }
}
