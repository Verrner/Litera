using System;

namespace Next.Litera
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class DataTransportOutputAttribute : Attribute
    {
        public readonly string Key;

        public DataTransportOutputAttribute(string key)
        {
            Key = key;
        }
    }
}