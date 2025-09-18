using Next.Litera.BuiltIn.RuntimeElements;
using System;
using System.Reflection;

namespace Next.Litera.BuiltIn.RuntimeConnectionTypes
{
    public sealed class BruteDataTransportRuntimeConnectionType : DataTransportRuntimeConnectionType
    {
        protected override void Transport(MemberInfo output, MemberInfo input, RuntimeConnection connection)
        {
            try
            {
                LiteraRuntimeUtilities.TransferMembersInfo(output, input, connection.Output, connection.Input);
            }
            catch (Exception e)
            {
                throw new Exception($"Exception thrown when trying to brute transport data between connected ports. Exception: {e.Message}");
            }
        }
    }
}