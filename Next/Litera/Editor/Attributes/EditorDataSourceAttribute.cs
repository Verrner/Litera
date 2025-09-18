using System;

namespace Next.Litera
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class EditorDataSourceAttribute : Attribute
    {
        public readonly string Key;

        public EditorDataSourceAttribute(string key)
        {
            Key = key;
        }
    }
}