using System;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Jobbr.DevSupport.ReferencedVersionAsserter.Tests
{
    [TestClass]
    public class XmlDependencyConverterTests
    {
        [TestMethod]
        public void Input_WithNullNode_RaisesException()
        {
            Should.Throw<ArgumentNullException>(() => XmlDependencyConverter.Convert(null));
        }

        [TestMethod]
        public void Input_WithoutId_ReturnsNull()
        {
            var doc = new XmlDocument();
            var node = doc.CreateElement("dependency");
            node.SetAttribute("version", "1.0");

            var value = XmlDependencyConverter.Convert(node);

            value.ShouldBeNull();
        }

        [TestMethod]
        public void Input_WithoutVersion_ReturnsNull()
        {
            var doc = new XmlDocument();
            var node = doc.CreateElement("dependency");
            node.SetAttribute("id", "bla");

            var value = XmlDependencyConverter.Convert(node);

            value.ShouldBeNull();
        }

        [TestMethod]
        public void Input_WithIdAndVersion_ReturnsConversion()
        {
            var node = CreateXmlNode("id", "1.0");

            var value = XmlDependencyConverter.Convert(node);

            value.ShouldNotBeNull();
        }

        [TestMethod]
        public void Input_WithVersionPreTag_ReturnsCorrectVersion()
        {
            var node = CreateXmlNode("id", "1.2.3-pre12");

            var value = XmlDependencyConverter.Convert(node);

            value.ShouldNotBeNull();
            value.MinVersion.Major.ShouldBe(1);
            value.MinVersion.Minor.ShouldBe(2);
            value.MinVersion.Bugfix.ShouldBe(3);

            value.MinVersion.Tag.ShouldBe("pre12");
        }

        [TestMethod]
        public void Conversion_ForId_AsExpected()
        {
            var node = CreateXmlNode("packageX", "1.2");

            var dependency = XmlDependencyConverter.Convert(node);

            dependency.Name.ShouldBe("packageX");
            dependency.Version.ShouldBe("1.2");

            dependency.ShouldNotBeNull();
        }

        [TestMethod]
        public void VersionConversion_SingleVersion_Fixed()
        {
            var node = CreateXmlNode("packageX", "[1.2.3]");

            var dependency = XmlDependencyConverter.Convert(node);

            dependency.MinVersion.Major.ShouldBe(1);
            dependency.MinVersion.Minor.ShouldBe(2);
            dependency.MinVersion.Bugfix.ShouldBe(3);
            dependency.MinVersion.Inclusive.ShouldBeTrue();

            dependency.MaxVersion.Major.ShouldBe(1);
            dependency.MaxVersion.Minor.ShouldBe(2);
            dependency.MaxVersion.Bugfix.ShouldBe(3);
            dependency.MaxVersion.Inclusive.ShouldBeTrue();
        }

        [TestMethod]
        public void VersionConversion_SingleVersion_AtLeast()
        {
            var node = CreateXmlNode("packageX", "1.2.3");

            var dependency = XmlDependencyConverter.Convert(node);

            dependency.MinVersion.Major.ShouldBe(1);
            dependency.MinVersion.Minor.ShouldBe(2);
            dependency.MinVersion.Bugfix.ShouldBe(3);
            dependency.MinVersion.Inclusive.ShouldBeTrue();

            dependency.MaxVersion.ShouldBeNull();
        }

        [TestMethod]
        public void VersionConversion_Ranges_MinInclusive_NoMax()
        {
            var node = CreateXmlNode("packageX", "[1.0,)");

            var dependency = XmlDependencyConverter.Convert(node);

            dependency.MinVersion.Major.ShouldBe(1);
            dependency.MinVersion.Minor.ShouldBe(0);
            dependency.MinVersion.Bugfix.ShouldBe(0);
            dependency.MinVersion.Inclusive.ShouldBeTrue();

            dependency.MaxVersion.ShouldBeNull();
        }

        [TestMethod]
        public void VersionConversion_Ranges_MinExclusive_NoMax()
        {
            var node = CreateXmlNode("packageX", "(1.0,)");

            var dependency = XmlDependencyConverter.Convert(node);

            dependency.MinVersion.Major.ShouldBe(1);
            dependency.MinVersion.Minor.ShouldBe(0);
            dependency.MinVersion.Bugfix.ShouldBe(0);
            dependency.MinVersion.Inclusive.ShouldBeFalse();

            dependency.MaxVersion.ShouldBeNull();
        }

        [TestMethod]
        public void VersionConversion_Ranges_NoMin_MaxInclusive()
        {
            var node = CreateXmlNode("packageX", "(,1.0]");

            var dependency = XmlDependencyConverter.Convert(node);

            dependency.MinVersion.ShouldBeNull();

            dependency.MaxVersion.Major.ShouldBe(1);
            dependency.MaxVersion.Minor.ShouldBe(0);
            dependency.MaxVersion.Bugfix.ShouldBe(0);
            dependency.MaxVersion.Inclusive.ShouldBeTrue();
        }

        [TestMethod]
        public void VersionConversion_Ranges_NoMin_ExclusiveMax()
        {
            var node = CreateXmlNode("packageX", "(,1.0)");

            var dependency = XmlDependencyConverter.Convert(node);

            dependency.MinVersion.ShouldBeNull();

            dependency.MaxVersion.Major.ShouldBe(1);
            dependency.MaxVersion.Minor.ShouldBe(0);
            dependency.MaxVersion.Bugfix.ShouldBe(0);
            dependency.MaxVersion.Inclusive.ShouldBeFalse();
        }

        [TestMethod]
        public void VersionConversion_Ranges_ExactRange_NoInclusion()
        {
            var node = CreateXmlNode("packageX", "(1.0, 2.0)");

            var dependency = XmlDependencyConverter.Convert(node);

            dependency.MinVersion.ShouldNotBeNull();
            dependency.MaxVersion.ShouldNotBeNull();

            dependency.MinVersion.Major.ShouldBe(1);
            dependency.MinVersion.Minor.ShouldBe(0);
            dependency.MinVersion.Bugfix.ShouldBe(0);

            dependency.MaxVersion.Major.ShouldBe(2);
            dependency.MaxVersion.Minor.ShouldBe(0);
            dependency.MaxVersion.Bugfix.ShouldBe(0);

            dependency.MinVersion.Inclusive.ShouldBeFalse();
            dependency.MaxVersion.Inclusive.ShouldBeFalse();
        }

        [TestMethod]
        public void VersionConversion_Ranges_MissingBracesAtBeginning_ThrowsException()
        {
            var node = CreateXmlNode("packageX", "1.0, 2.0)");

            Should.Throw<ArgumentException>(() => XmlDependencyConverter.Convert(node));
        }

        [TestMethod]
        public void VersionConversion_Ranges_MissingBracesOnEnd_ThrowsException()
        {
            var node = CreateXmlNode("packageX", "(1.0, 2.0");

            Should.Throw<ArgumentException>(() => XmlDependencyConverter.Convert(node));
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
