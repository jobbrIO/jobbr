using System;
using System.Net.Mail;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.DevSupport.ReferencedVersionAsserter.Tests
{
    [TestClass]
    public class XmlDependencyConverterTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Input_WithNullNode_RaisesException()
        {
            XmlDependencyConverter.Convert(null);
        }

        [TestMethod]
        public void Input_WithoutId_ReturnsNull()
        {
            var doc = new XmlDocument();
            var node = doc.CreateElement("dependency");
            node.SetAttribute("version", "1.0");

            var value = XmlDependencyConverter.Convert(node);

            Assert.IsNull(value);
        }

        [TestMethod]
        public void Input_WithoutVersion_ReturnsNull()
        {
            var doc = new XmlDocument();
            var node = doc.CreateElement("dependency");
            node.SetAttribute("id", "bla");

            var value = XmlDependencyConverter.Convert(node);

            Assert.IsNull(value);
        }

        [TestMethod]
        public void Input_WithIdAndVersion_ReturnsConversion()
        {
            var node = CreateXmlNode("id", "1.0");

            var value = XmlDependencyConverter.Convert(node);

            Assert.IsNotNull(value);
        }

        [TestMethod]
        public void Conversion_ForId_AsExpected()
        {
            var node = CreateXmlNode("packageX", "1.2");

            var dependency = XmlDependencyConverter.Convert(node);

            Assert.AreEqual("packageX", dependency.Name);
            Assert.AreEqual("1.2", dependency.Version);

            Assert.IsNotNull(dependency);
        }

        [TestMethod]
        public void VersionConversion_SingleVersion_Fixed()
        {
            var node = CreateXmlNode("packageX", "[1.2.3]");

            var dependency = XmlDependencyConverter.Convert(node);

            Assert.AreEqual(1, dependency.MinVersion.Major);
            Assert.AreEqual(2, dependency.MinVersion.Minor);
            Assert.AreEqual(3, dependency.MinVersion.Bugfix);
            Assert.IsTrue(dependency.MinVersion.Inclusive);

            Assert.AreEqual(1, dependency.MaxVersion.Major);
            Assert.AreEqual(2, dependency.MaxVersion.Minor);
            Assert.AreEqual(3, dependency.MaxVersion.Bugfix);
            Assert.IsTrue(dependency.MaxVersion.Inclusive);
        }

        [TestMethod]
        public void VersionConversion_SingleVersion_AtLeast()
        {
            var node = CreateXmlNode("packageX", "1.2.3");

            var dependency = XmlDependencyConverter.Convert(node);

            Assert.AreEqual(1, dependency.MinVersion.Major);
            Assert.AreEqual(2, dependency.MinVersion.Minor);
            Assert.AreEqual(3, dependency.MinVersion.Bugfix);
            Assert.IsTrue(dependency.MinVersion.Inclusive);

            Assert.IsNull(dependency.MaxVersion);
        }

        [TestMethod]
        public void VersionConversion_Ranges_MinInclusive_NoMax()
        {
            var node = CreateXmlNode("packageX", "[1.0,)");

            var dependency = XmlDependencyConverter.Convert(node);

            Assert.AreEqual(1, dependency.MinVersion.Major);
            Assert.AreEqual(0, dependency.MinVersion.Minor);
            Assert.AreEqual(0, dependency.MinVersion.Bugfix);
            Assert.IsTrue(dependency.MinVersion.Inclusive);

            Assert.IsNull(dependency.MaxVersion);
        }

        [TestMethod]
        public void VersionConversion_Ranges_MinExlusive_NoMax()
        {
            var node = CreateXmlNode("packageX", "(1.0,)");

            var dependency = XmlDependencyConverter.Convert(node);

            Assert.AreEqual(1, dependency.MinVersion.Major);
            Assert.AreEqual(0, dependency.MinVersion.Minor);
            Assert.AreEqual(0, dependency.MinVersion.Bugfix);
            Assert.AreEqual(false, dependency.MinVersion.Inclusive);

            Assert.IsNull(dependency.MaxVersion);
        }

        [TestMethod]
        public void VersionConversion_Ranges_NoMin_MaxInclusive()
        {
            var node = CreateXmlNode("packageX", "(,1.0]");

            var dependency = XmlDependencyConverter.Convert(node);

            Assert.IsNull(dependency.MinVersion);

            Assert.AreEqual(1, dependency.MaxVersion.Major);
            Assert.AreEqual(0, dependency.MaxVersion.Minor);
            Assert.AreEqual(0, dependency.MaxVersion.Bugfix);
            Assert.AreEqual(true, dependency.MaxVersion.Inclusive);
        }

        [TestMethod]
        public void VersionConversion_Ranges_NoMin_ExlusiveMax()
        {
            var node = CreateXmlNode("packageX", "(,1.0)");

            var dependency = XmlDependencyConverter.Convert(node);

            Assert.IsNull(dependency.MinVersion);

            Assert.AreEqual(1, dependency.MaxVersion.Major);
            Assert.AreEqual(0, dependency.MaxVersion.Minor);
            Assert.AreEqual(0, dependency.MaxVersion.Bugfix);
            Assert.AreEqual(false, dependency.MaxVersion.Inclusive);
        }

        [TestMethod]
        public void VersionConversion_Ranges_ExactRange_NoInclusion()
        {
            var node = CreateXmlNode("packageX", "(1.0, 2.0)");

            var dependency = XmlDependencyConverter.Convert(node);

            Assert.IsNotNull(dependency.MinVersion);
            Assert.IsNotNull(dependency.MaxVersion);

            Assert.AreEqual(1, dependency.MinVersion.Major);
            Assert.AreEqual(0, dependency.MinVersion.Minor);
            Assert.AreEqual(0, dependency.MinVersion.Bugfix);

            Assert.AreEqual(2, dependency.MaxVersion.Major);
            Assert.AreEqual(0, dependency.MaxVersion.Minor);
            Assert.AreEqual(0, dependency.MaxVersion.Bugfix);

            Assert.IsFalse(dependency.MinVersion.Inclusive);
            Assert.IsFalse(dependency.MaxVersion.Inclusive);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void VersionConversion_Ranges_MissingBracesAtBeginning_ThrowsExecption()
        {
            var node = CreateXmlNode("packageX", "1.0, 2.0)");

            XmlDependencyConverter.Convert(node);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void VersionConversion_Ranges_MissingBracesOnEnd_ThrowsExecption()
        {
            var node = CreateXmlNode("packageX", "(1.0, 2.0");

            XmlDependencyConverter.Convert(node);
        }

        private static XmlElement CreateXmlNode(string id, string version)
        {
            var doc = new XmlDocument();
            var node = doc.CreateElement("dependency");
            node.SetAttribute("id", id);
            node.SetAttribute("version", version);
            return node;
        }
    }
}
