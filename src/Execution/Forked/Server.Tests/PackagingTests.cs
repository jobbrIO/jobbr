using Jobbr.DevSupport.ReferencedVersionAsserter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.Server.ForkedExecution.Tests
{
    [TestClass]
    public class PackagingTests
    {
        [TestMethod]
        [Ignore("Rewrite as project reference test")]
        public void Feature_NuSpec_IsCompliant()
        {
            var asserter = new Asserter(Asserter.ResolveProjectFile("Jobbr.Server.ForkedExecution", "Jobbr.Server.ForkedExecution.csproj"), Asserter.ResolveRootFile("Jobbr.Execution.Forked.nuspec"));

            asserter.Add(new PackageExistsInBothRule("Jobbr.ComponentModel.Registration"));
            asserter.Add(new PackageExistsInBothRule("Jobbr.ComponentModel.Execution"));
            asserter.Add(new PackageExistsInBothRule("SimpleInjector"));
            asserter.Add(new PackageExistsInBothRule("System.Text.Json"));

            asserter.Add(new VersionIsIncludedInRange("Jobbr.ComponentModel.*"));
            asserter.Add(new VersionIsIncludedInRange("SimpleInjector"));
            asserter.Add(new VersionIsIncludedInRange("System.Text.Json"));

            asserter.Add(new NoMajorChangesInNuSpec("Jobbr.*"));

            var result = asserter.Validate();

            Assert.IsTrue(result.IsSuccessful, result.Message);
        }

        [TestMethod]
        [Ignore("Rewrite as project reference test")]
        public void Runtime_NuSpec_IsCompliant()
        {
            var asserter = new Asserter(Asserter.ResolveProjectFile("Jobbr.Runtime.ForkedExecution", "Jobbr.Runtime.ForkedExecution.csproj"), Asserter.ResolveRootFile("Jobbr.Runtime.ForkedExecution.nuspec"));

            asserter.Add(new PackageExistsInBothRule("CommandLineParser"));
            asserter.Add(new PackageExistsInBothRule("Microsoft.Extensions.Logging.Abstractions"));
            asserter.Add(new PackageExistsInBothRule("System.Text.Json"));

            asserter.Add(new VersionIsIncludedInRange("CommandLineParser"));
            asserter.Add(new VersionIsIncludedInRange("Microsoft.Extensions.Logging.Abstractions"));
            asserter.Add(new VersionIsIncludedInRange("System.Text.Json"));

            var result = asserter.Validate();

            Assert.IsTrue(result.IsSuccessful, result.Message);
        }
    }
}
