using Jobbr.ComponentModel.ArtefactStorage;
using Jobbr.ComponentModel.ArtefactStorage.Model;
using MimeTypes;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Jobbr.ArtefactStorage.FileSystem
{
    /// <summary>
    /// Stores Jobbr artefacts in the file system.
    /// </summary>
    public class FileSystemArtefactsStorageProvider : IArtefactsStorageProvider
    {
        private readonly FileSystemArtefactStorageConfiguration _config;

        public FileSystemArtefactsStorageProvider(FileSystemArtefactStorageConfiguration config)
        {
            _config = config;
        }

        public void Save(string container, string fileName, Stream content)
        {
            var dir = Directory.CreateDirectory(Path.Combine(_config.DataDirectory, container));
            var fileFullPath = Path.Combine(dir.FullName, fileName);

            if (File.Exists(fileFullPath))
            {
                File.Delete(fileFullPath);
            }

            using (var fileStream = File.Create(fileFullPath))
            {
                content.CopyTo(fileStream);
            }
        }

        public Stream Load(string container, string fileName)
        {
            var dir = Directory.CreateDirectory(Path.Combine(_config.DataDirectory, container));
            var fileFullPath = Path.Combine(dir.FullName, fileName);

            if (File.Exists(fileFullPath))
            {
                return File.OpenRead(fileFullPath);
            }

            return null;
        }

        public List<JobbrArtefact> GetArtefacts(string container)
        {
            var dir = Directory.CreateDirectory(Path.Combine(_config.DataDirectory, container));

            return dir.GetFiles().Select(s => new JobbrArtefact { FileName = s.Name, Size = s.Length, MimeType = MimeTypeMap.GetMimeType(s.Extension) }).ToList();
        }
    }
}
