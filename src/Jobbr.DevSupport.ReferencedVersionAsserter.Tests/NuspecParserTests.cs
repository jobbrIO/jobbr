using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Jobbr.DevSupport.ReferencedVersionAsserter.Tests
{
    [TestClass]
    public class NuspecParserTests
    {
        private const string TestFilePath = "TestFiles/NuSpecHttpListenerDependency.nuspec";

        [TestMethod]
        public void SingleDependency_WhenLoaded_Counted()
        {
            var loader = new NuspecParser(TestFilePath);

            loader.Dependencies.Count.ShouldBe(1);
        }

        [TestMethod]
        public void SingleDependency_WhenLoaded_IdEqualsName()
        {
            var loader = new NuspecParser(TestFilePath);

            loader.Dependencies.Count.ShouldBe(1);
            loader.Dependencies[0].Name.ShouldBe("Microsoft.Owin.Host.HttpListener");
        }

        [TestMethod]
        public void SingleDependency_WhenLoaded_VersionRangeMatches()
        {
            var loader = new NuspecParser(TestFilePath);

            loader.Dependencies.Count.ShouldBe(1);
            loader.Dependencies[0].Name.ShouldBe("Microsoft.Owin.Host.HttpListener");

            loader.Dependencies[0].MinVersion.Major.ShouldBe(3);
            loader.Dependencies[0].MaxVersion.Major.ShouldBe(4);
        }
        
        [TestMethod]
        public void SingleGroupedDependency_WhenLoaded_Counted()
        {
            var loader = new NuspecParser("TestFiles/NuSpecHttpListenerGroupedDependency.nuspec");

            loader.Dependencies.Count.ShouldBe(1);
        }
    }
}
