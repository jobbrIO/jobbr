using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.DevSupport.ReferencedVersionAsserter.Tests
{
    [TestClass]
    public class ProjectParserTests
    {
        private string testFilePath = "TestFiles/ProjectSDK.csproj";

        [TestMethod]
        public void SingleDependency_WhenLoaded_Counted()
        {
            var parser = new ProjectParser(testFilePath);

            Assert.AreEqual(1, parser.Dependencies.Count);
        }

        [TestMethod]
        public void SingleDependency_WhenLoaded_IdEqualsName()
        {
            var parser = new ProjectParser(testFilePath);

            Assert.AreEqual(1, parser.Dependencies.Count);
            Assert.AreEqual("Microsoft.Owin.Host.HttpListener", parser.Dependencies[0].Name);
        }

        [TestMethod]
        public void SingleDependency_WhenLoaded_VersionRangeMatches()
        {
            var parser = new ProjectParser(testFilePath);

            Assert.AreEqual(1, parser.Dependencies.Count);
            Assert.AreEqual("Microsoft.Owin.Host.HttpListener", parser.Dependencies[0].Name);

            Assert.AreEqual(3, parser.Dependencies[0].MinVersion.Major);
            Assert.AreEqual(0, parser.Dependencies[0].MinVersion.Minor);
            Assert.AreEqual(1, parser.Dependencies[0].MinVersion.Bugfix);
        }
    }
}
