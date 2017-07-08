using System;
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
            node.SetAttribute("version", "bla");

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
            var node = CreateXmlNode("id", "bla");

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
