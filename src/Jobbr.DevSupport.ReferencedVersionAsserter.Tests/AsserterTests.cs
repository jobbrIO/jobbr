using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.DevSupport.ReferencedVersionAsserter.Tests
{
    [TestClass]
    public class AsserterTests
    {
        [TestMethod]
        public void Asserter_WithExactVersionRule_ValidatesExactVersion()
        {
            var asserter = new Asserter("TestFiles/ExactDependency.config", "TestFiles/ExactDependency.nuspec");

            var result = asserter.Validate(new ExactVersionMatchRule("ExactDependency"));

            Assert.IsTrue(result.IsSuccessful);
        }
    }
}