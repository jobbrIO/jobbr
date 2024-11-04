using Jobbr.DevSupport.ReferencedVersionAsserter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.Storage.MsSql.Tests
{
    [TestClass]
    public class PackagingTests
    {
        [TestMethod]
        public void Feature_NuSpec_IsCompliant()
        {
            var asserter = new Asserter(Asserter.ResolveProjectFile("Jobbr.Storage.MsSql", "Jobbr.Storage.MsSql.csproj"), Asserter.ResolveRootFile("Jobbr.Storage.MsSql.nuspec"));

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
