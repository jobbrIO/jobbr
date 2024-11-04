using Jobbr.Server.ForkedExecution.Execution.ServiceMessaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.Server.ForkedExecution.Tests
{
    /// <summary>
    /// Based on: https://confluence.jetbrains.com/display/TCD8/Build+Script+Interaction+with+TeamCity
    /// <code>
    ///     ##jobbr[<messageName> name1='value1' name2='value2']
    /// </code>
    /// </summary>
    [TestClass]
    public class ServiceMessagesParserTests
    {
        [TestMethod]
        public void MessageWithDoubleValue_WhenParsed_ContainsValue()
        {
            var parser = new ServiceMessageParser();

            const string raw = "##jobbr[double value='55.34']";

            var message = (DoubleServiceMessage)parser.Parse(raw);

            Assert.IsNotNull(message);
            Assert.AreEqual(55.34, message.Value);
        }

        [TestMethod]
        public void MessageWitIntValue_WhenParsed_ContainsValue()
        {
            var parser = new ServiceMessageParser();

            const string raw = "##jobbr[integer value='57']";

            var message = (IntegerServiceMessage)parser.Parse(raw);

            Assert.IsNotNull(message);
            Assert.AreEqual(57, message.Value);
        }

        [TestMethod]
        public void MessageWitStringValue_WhenParsed_ContainsValue()
        {
            var parser = new ServiceMessageParser();

            const string raw = "##jobbr[string value='hello world']";

            var message = (StringServiceMessage)parser.Parse(raw);

            Assert.IsNotNull(message);
            Assert.AreEqual("hello world", message.Value);
        }

        [TestMethod]
        public void UnknownMessageType_WhenParsed_ReturnsNull()
        {
            var parser = new ServiceMessageParser();

            const string raw = "##jobbr[blabla value='57']";

            var message = parser.Parse(raw);

            Assert.IsNull(message);
        }

        public class DoubleServiceMessage : ServiceMessage
        {
            public double Value { get; set; }
        }

        public class IntegerServiceMessage : ServiceMessage
        {
            public int Value { get; set; }
        }

        public class StringServiceMessage : ServiceMessage
        {
            public string Value { get; set; }
        }
    }
}
