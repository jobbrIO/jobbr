using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Jobbr.DevSupport.ReferencedVersionAsserter.Tests
{
    [TestClass]
    public class PackagesParserTests
    {
        private const string TestFilePath = "TestFiles/packagesWithHttpListenerDependency.config";

        [TestMethod]
        public void SingleDependency_WhenLoaded_Counted()
        {
            var parser = new PackagesParser(TestFilePath);

            parser.Dependencies.Count.ShouldBe(1);
        }

        [TestMethod]
        public void SingleDependency_WhenLoaded_IdEqualsName()
        {
            var parser = new PackagesParser(TestFilePath);

            parser.Dependencies.Count.ShouldBe(1);
            parser.Dependencies[0].Name.ShouldBe("Microsoft.Owin.Host.HttpListener");
        }

        [TestMethod]
        public void SingleDependency_WhenLoaded_VersionRangeMatches()
        {
            var parser = new PackagesParser(TestFilePath);

            parser.Dependencies.Count.ShouldBe(1);
            parser.Dependencies[0].Name.ShouldBe("Microsoft.Owin.Host.HttpListener");

            parser.Dependencies[0].MinVersion.Major.ShouldBe(4);
            parser.Dependencies[0].MinVersion.Minor.ShouldBe(2);
            parser.Dependencies[0].MinVersion.Bugfix.ShouldBe(2);
        }
    }
}
