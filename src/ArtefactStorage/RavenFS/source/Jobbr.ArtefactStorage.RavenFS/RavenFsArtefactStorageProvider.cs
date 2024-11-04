using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Jobbr.ComponentModel.ArtefactStorage;
using Jobbr.ComponentModel.ArtefactStorage.Model;
using MimeTypes;
using Raven.Client.FileSystem;
using Raven.Json.Linq;

namespace Jobbr.ArtefactStorage.RavenFS
{
    public class RavenFsArtefactStorageProvider : IArtefactsStorageProvider
    {
        private readonly FilesStore _filesStore;

        public RavenFsArtefactStorageProvider(RavenFsConfiguration configuration)
        {
            _filesStore = new FilesStore
            {
                Url = configuration.Url,
                DefaultFileSystem = configuration.FileSystem
            };

            _filesStore.Initialize();
        }

        public void Save(string container, string fileName, Stream content)
        {
            AsyncHelper.RunSync(() => SaveAsync(container, fileName, content));
        }

        public async Task SaveAsync(string container, string fileName, Stream content)
        {
            using (var session = _filesStore.OpenAsyncSession())
            {
                var metadata = new RavenJObject
                {
                    { "Container", container },
                    { "Name", fileName }
                };

                session.RegisterUpload($"{container}/{fileName}", content, metadata);
                await session.SaveChangesAsync();
            }
        }

        public async Task<Stream> LoadAsync(string container, string fileName)
        {
            using (var session = _filesStore.OpenAsyncSession())
            {
                var file = await session.Query()
                    .WhereEquals("Container", container)
                    .WhereEquals("Name", fileName)
                    .FirstOrDefaultAsync();

                var stream = await session.DownloadAsync(file.Name);

                return stream;
            }
        }

        public List<JobbrArtefact> GetArtefacts(string container)
        {
            return AsyncHelper.RunSync(() => GetArtefactsAsync(container));
        }

        public async Task<List<JobbrArtefact>> GetArtefactsAsync(string container)
        {
            using (var session = _filesStore.OpenAsyncSession())
            {
                var files = await session.Query()
                    .WhereEquals("Container", container)
                    .ToListAsync();

                var list = new List<JobbrArtefact>();

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var file in files)
                {
                    var fileName = file.Metadata["Name"].Value<string>();
                    var size = file.Metadata["RavenFS-Size"].Value<long>();
                    var extension = fileName.ExtractExtension();
                    var mimeType = MimeTypeMap.GetMimeType(extension);

                    list.Add(new JobbrArtefact
                    {
                        FileName = fileName,
                        Size = size,
                        MimeType = mimeType
                    });
                }

                return list;
            }
        }

        public Stream Load(string container, string fileName)
        {
            return AsyncHelper.RunSync(() => LoadAsync(container, fileName));
        }
    }
}
