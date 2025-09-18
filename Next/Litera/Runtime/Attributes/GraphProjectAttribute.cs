using System;

namespace Next.Litera
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GraphProjectAttribute : Attribute
    {
        public readonly Type PatchType;

        public GraphProjectAttribute(Type patchType)
        {
            PatchType = patchType;   
        }
    }
}