using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.DevSupport.ReferencedVersionAsserter.Tests
{
    [TestClass]
    public class PackagesParserTests
    {
        private string testFilePath = "TestFiles/packagesWithHttpListenerDependency.config";

        [TestMethod]
        public void SingleDependency_WhenLoaded_Counted()
        {
            var parser = new PackagesParser(testFilePath);

            Assert.AreEqual(1, parser.Dependencies.Count);
        }

        [TestMethod]
        public void SingleDependency_WhenLoaded_IdEqualsName()
        {
            var parser = new PackagesParser(testFilePath);

            Assert.AreEqual(1, parser.Dependencies.Count);
            Assert.AreEqual("Microsoft.Owin.Host.HttpListener", parser.Dependencies[0].Name);
        }

        [TestMethod]
        public void SingleDependency_WhenLoaded_VersionRangeMatches()
        {
            var parser = new PackagesParser(testFilePath);

            Assert.AreEqual(1, parser.Dependencies.Count);
            Assert.AreEqual("Microsoft.Owin.Host.HttpListener", parser.Dependencies[0].Name);

            Assert.AreEqual(4, parser.Dependencies[0].MinVersion.Major);
            Assert.AreEqual(2, parser.Dependencies[0].MinVersion.Minor);
            Assert.AreEqual(2, parser.Dependencies[0].MinVersion.Bugfix);
        }
    }
}
