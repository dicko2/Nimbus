#if NET462
using System.Runtime.Remoting.Messaging;
#endif

namespace Nimbus.Infrastructure.Logging
{
    internal static class DispatchLoggingContext
    {
        internal static NimbusMessage NimbusMessage
        {
#if NET462
            get { return System.Runtime.Remoting.Messaging.CallContext.LogicalGetData("NimbusMessage") as NimbusMessage; }
            set { System.Runtime.Remoting.Messaging.CallContext.LogicalSetData("NimbusMessage", value); }
#else
            get { return CallContext.LogicalGetData("NimbusMessage") as NimbusMessage; }
            set { CallContext.LogicalSetData("NimbusMessage", value); }
#endif
        }
    }
}