using System;

namespace Next.Litera
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GraphWindowInfoAttribute : Attribute
    {
        public readonly string[] Collections;
        public readonly Type ProjectFileType;
        public readonly bool EnableAllCollections;

        public GraphWindowInfoAttribute(Type projectFileType, params string[] collections) : this(projectFileType)
        {
            Collections = collections;
            EnableAllCollections = false;
        }

        public GraphWindowInfoAttribute(Type projectFileType)
        {
            ProjectFileType = projectFileType;
            EnableAllCollections = true;
        }
    }
}