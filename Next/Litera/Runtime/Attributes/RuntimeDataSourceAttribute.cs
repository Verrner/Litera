using System;

namespace Next.Litera
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class RuntimeDataSourceAttribute : Attribute
    {
        public readonly string Key;

        public RuntimeDataSourceAttribute(string key)
        {
            Key = key;
        }
    }
}