using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.DevSupport.ReferencedVersionAsserter.Tests
{
    [TestClass]
    public class AsserterTests
    {
        [TestMethod]
        void Asserter_WithExactVersionRule_ValidatesExactVersion()
        {
            var asserter = new Asserter("TestFiles/ExactDependency.config", "TextFiles/ExactDependency.nuspec");

            var result = asserter.Validate(new ExactVersionMatchRule("ExactDependency"));

            Assert.IsTrue(result.IsSuccessful);
        }
    }
}