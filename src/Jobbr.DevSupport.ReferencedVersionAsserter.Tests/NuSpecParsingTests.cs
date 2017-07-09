using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.DevSupport.ReferencedVersionAsserter.Tests
{
    [TestClass]
    public class NuSpecParsingTests
    {
        [TestMethod]
        public void SingleDependency_WhenLoaded_Counted()
        {
            var loader = new NuSpecDependencyParser("NuSpecHttpListenerDependency.nuspec");

            Assert.AreEqual(1, loader.Dependencies.Count);
        }

        [TestMethod]
        public void SingleDependency_WhenLoaded_IdEqualsName()
        {
            var loader = new NuSpecDependencyParser("NuSpecHttpListenerDependency.nuspec");

            Assert.AreEqual(1, loader.Dependencies.Count);
            Assert.AreEqual("Microsoft.Owin.Host.HttpListener", loader.Dependencies[0].Name);
        }

        [TestMethod]
        public void SingleDependency_WhenLoaded_VersionRangeMatches()
        {
            var loader = new NuSpecDependencyParser("NuSpecHttpListenerDependency.nuspec");

            Assert.AreEqual(1, loader.Dependencies.Count);
            Assert.AreEqual("Microsoft.Owin.Host.HttpListener", loader.Dependencies[0].Name);

            Assert.AreEqual(3, loader.Dependencies[0].MinVersion.Major);
            Assert.AreEqual(4, loader.Dependencies[0].MaxVersion.Major);
        }

    }
}
