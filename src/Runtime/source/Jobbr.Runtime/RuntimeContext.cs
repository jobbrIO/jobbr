using System;

namespace Jobbr.Runtime
{
    [Obsolete("Please don't depend on RuntimeContext anymore and depend on Thread.CurrentPrincipal instead.")]
    public class RuntimeContext
    {
        [Obsolete("Please use Thread.CurrentPrincipal.Identity to get the user's current Identity")]
        public string UserId { get; set; }

        [Obsolete("UserDisplayName will be deprecated soon. Please use Thread.CurrentPrincipal.Identity to get access to the user's identity")]
        public string UserDisplayName { get; set; }
    }
}
