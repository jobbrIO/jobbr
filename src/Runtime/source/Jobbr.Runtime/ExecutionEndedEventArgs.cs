using System;

namespace Jobbr.Runtime
{
    public class ExecutionEndedEventArgs
    {
        public bool Succeeded { get; set; }

        public Exception Exception { get; set; }
    }
}