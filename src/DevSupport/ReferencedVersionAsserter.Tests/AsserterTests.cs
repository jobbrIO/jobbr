using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Jobbr.DevSupport.ReferencedVersionAsserter.Tests
{
    [TestClass]
    public class AsserterTests
    {
        [TestMethod]
        public void WithExactVersionRule_ExactMatch_IsSuccessful()
        {
            var asserter = new Asserter("TestFiles/ExactDependencyV5.config", "TestFiles/ExactDependencyV5.nuspec");

            var result = asserter.Validate(new ExactVersionMatchRule("ExactDependency"));

            result.IsSuccessful.ShouldBeTrue();
        }

        [TestMethod]
        public void WithExactVersionRule_DifferentVersions_Fails()
        {
            var asserter = new Asserter("TestFiles/ExactDependencyV2.config", "TestFiles/ExactDependencyV5.nuspec");

            var result = asserter.Validate(new ExactVersionMatchRule("ExactDependency"));

            result.Messages.Count.ShouldBe(1);
            result.IsSuccessful.ShouldBeFalse();
        }

        [TestMethod]
        public void WithExactVersionRule_WildChar_Successful()
        {
            var asserter = new Asserter("TestFiles/ExactDependencyV5.config", "TestFiles/ExactDependencyV5.nuspec");

            var result = asserter.Validate(new ExactVersionMatchRule("ExactDepend*"));

            result.Messages.Count.ShouldBe(0);
            result.IsSuccessful.ShouldBeTrue();
        }
    }
}