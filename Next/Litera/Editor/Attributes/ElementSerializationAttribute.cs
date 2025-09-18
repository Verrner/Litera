using Next.Litera.Scripting;
using System;

namespace Next.Litera
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ElementSerializationAttribute : Attribute
    {
        public readonly Type RuntimeTargetType;
        public readonly int SerializationOrder;

        public ElementSerializationAttribute(Type runtimeTargetType, int serializationOrder = 0)
        {
            if (!runtimeTargetType.IsSubclassOf(typeof(RuntimeGraphElement))) throw new Exception($"Runtime target type {runtimeTargetType} is not inherited from {typeof(RuntimeGraphElement)}.");
            RuntimeTargetType = runtimeTargetType;
            SerializationOrder = serializationOrder;
        }
    }
}