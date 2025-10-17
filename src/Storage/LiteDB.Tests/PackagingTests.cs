using Jobbr.DevSupport.ReferencedVersionAsserter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.Storage.LiteDB.Tests
{
    [TestClass]
    public class PackagingTests
    {
        [TestMethod]
        [Ignore("Rewrite as project reference test")]
        public void Feature_NuSpec_IsCompliant()
        {
            var asserter = new Asserter(Asserter.ResolveProjectFile("Jobbr.Storage.LiteDB", "Jobbr.Storage.LiteDB.csproj"), Asserter.ResolveRootFile("Jobbr.Storage.LiteDB.nuspec"));

            asserter.Add(new PackageExistsInBothRule("Jobbr.ComponentModel.Registration"));
            asserter.Add(new PackageExistsInBothRule("Jobbr.ComponentModel.JobStorage"));
            asserter.Add(new PackageExistsInBothRule("Microsoft.Extensions.Logging.Abstractions"));

            asserter.Add(new VersionIsIncludedInRange("Jobbr.ComponentModel.*"));
            asserter.Add(new VersionIsIncludedInRange("Microsoft.Extensions.Logging.Abstractions"));

            asserter.Add(new NoMajorChangesInNuSpec("Jobbr.*"));

            var result = asserter.Validate();

            Assert.IsTrue(result.IsSuccessful, result.Message);
        }
    }
}