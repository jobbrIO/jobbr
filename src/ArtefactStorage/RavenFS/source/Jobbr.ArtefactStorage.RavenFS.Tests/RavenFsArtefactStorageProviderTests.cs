using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.ArtefactStorage.RavenFS.Tests
{
    [TestClass]
    public class RavenFsArtefactStorageProviderTests : IntegrationTestBase
    {
        [TestMethod]
        public void GivenRavenFs_WhenQueryingForNonExistingContainer_ReturnsEmptyList()
        {
            GivenRavenFs();
            GivenRavenFsConfiguration();

            var storageProvider = new RavenFsArtefactStorageProvider(RavenFsConfiguration);
         
            var artefacts = storageProvider.GetArtefacts("non-existing-container");

            Assert.AreEqual(0, artefacts.Count);
        }

        [TestMethod]
        public void GivenOneFileInContainer_WhenQueryingAllFilesFromContainer_ReturnsThatFile()
        {
            GivenRavenFs();
            GivenRavenFsConfiguration();

            var storageProvider = new RavenFsArtefactStorageProvider(RavenFsConfiguration);

            const string text = "lorem ipsum";
            storageProvider.Save("test-container", "file1.txt", new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text)));

            WaitForIndexing(Store);

            var test = storageProvider.GetArtefacts("test-container");

            Assert.AreEqual(1, test.Count, "file1.txt must be in the list");
            Assert.AreEqual("file1.txt", test[0].FileName);
        }

        [TestMethod]
        public void GivenMultipleFilesInContainer_WhenQueryingAllFilesFromContainer_ReturnsAllFiles()
        {
            GivenRavenFs();
            GivenRavenFsConfiguration();

            var storageProvider = new RavenFsArtefactStorageProvider(RavenFsConfiguration);

            const string text1 = "lorem ipsum";
            const string text2 = "blub";
            storageProvider.Save("test-container", "file1.txt", new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text1)));
            storageProvider.Save("test-container", "file2.txt", new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text2)));

            WaitForIndexing(Store, RavenFsConfiguration.FileSystem);

            var test = storageProvider.GetArtefacts("test-container");

            Assert.AreEqual(2, test.Count, "both files must be in the list");
        }

        [TestMethod]
        public void GivenMultipleFilesInContainer_WhenQueryingAllFilesFromAnotherContainer_ReturnsNoFiles()
        {
            GivenRavenFs();
            GivenRavenFsConfiguration();

            var storageProvider = new RavenFsArtefactStorageProvider(RavenFsConfiguration);

            const string text1 = "lorem ipsum";
            const string text2 = "blub";
            storageProvider.Save("test-container", "file1.txt", new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text1)));
            storageProvider.Save("test-container", "file2.txt", new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text2)));

            WaitForIndexing(Store);

            var test = storageProvider.GetArtefacts("another-container");

            Assert.AreEqual(0, test.Count, "none of the items in test-container should be in the list");
        }

        [TestMethod]
        public void GivenPlainTextFile_WhenQueryingThatFile_MimeTypeIsProperlySet()
        {
            GivenRavenFs();
            GivenRavenFsConfiguration();

            var storageProvider = new RavenFsArtefactStorageProvider(RavenFsConfiguration);

            const string text = "lorem ipsum";
            storageProvider.Save("test-container", "file1.txt", new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text)));

            WaitForIndexing(Store);

            var test = storageProvider.GetArtefacts("test-container");

            Assert.AreEqual(1, test.Count, "file1.txt must be in the list");
            Assert.AreEqual("text/plain", test[0].MimeType);
        }

        [TestMethod]
        public void GivenPlainTextFileWithSize11_WhenQueryingThatFile_SizeIsProperlySet()
        {
            GivenRavenFs();
            GivenRavenFsConfiguration();

            var storageProvider = new RavenFsArtefactStorageProvider(RavenFsConfiguration);

            const string text = "lorem ipsum";
            storageProvider.Save("test-container", "file1.txt", new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text)));

            WaitForIndexing(Store);

            var test = storageProvider.GetArtefacts("test-container");

            Assert.AreEqual(1, test.Count, "file1.txt must be in the list");
            Assert.AreEqual(11, test[0].Size);
        }
    }
}