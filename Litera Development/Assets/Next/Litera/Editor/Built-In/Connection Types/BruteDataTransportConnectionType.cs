using Next.Litera.BuiltIn.RuntimeConnectionTypes;
using Next.Litera.Scripting;
using UnityEngine.UIElements;

namespace Next.Litera.BuiltIn.ConnectionTypes
{
    [ConnectionType("Brute Data Transport", typeof(BruteDataTransportRuntimeConnectionType))]
    public sealed class BruteDataTransportConnectionType : DataTransportConnectionType
    {
        protected override void DrawConnectionType(Foldout parentFoldout)
        {

        }
    }
}