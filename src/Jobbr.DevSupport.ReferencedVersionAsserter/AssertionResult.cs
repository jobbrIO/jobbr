using System.Collections.Generic;
using static System.String;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    /// <summary>
    /// Result class for the assertions.
    /// </summary>
    public class AssertionResult
    {
        /// <summary>
        /// Add assertion message.
        /// </summary>
        /// <param name="name">Rule type name.</param>
        /// <param name="message">Assertion message.</param>
        internal void AddMessage(string name, string message)
        {
            Messages.Add($"[{name}] {message}");
        }

        /// <summary>
        /// Assertion messages.
        /// </summary>
        public List<string> Messages { get; } = new List<string>();

        /// <summary>
        /// If assertion was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Assertion message.
        /// </summary>
        public string Message => "Reason(s) below:\n\n" + Join("\n", Messages) + "\n";
    }
}
