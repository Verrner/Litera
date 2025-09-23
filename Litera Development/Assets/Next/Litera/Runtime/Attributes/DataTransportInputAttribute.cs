using System;

namespace Next.Litera
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class DataTransportInputAttribute : Attribute
    {
        public readonly string Key;

        public DataTransportInputAttribute(string key)
        {
            Key = key;
        }
    }
}