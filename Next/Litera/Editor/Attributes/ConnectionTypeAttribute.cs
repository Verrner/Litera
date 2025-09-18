using System;

namespace Next.Litera
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ConnectionTypeAttribute : Attribute
    {
        public readonly string ConnectionName;
        public readonly Type RuntimeConnectionType;

        public ConnectionTypeAttribute(string name, Type runtimeType)
        {
            ConnectionName = name;
            RuntimeConnectionType = runtimeType;
        }
    }
}