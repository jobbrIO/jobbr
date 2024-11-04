using System.IO;

namespace Jobbr.ArtefactStorage.RavenFS.Tests
{
    public static class StreamExtensions
    {
        public static byte[] ReadInMemoryToEnd(this Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}