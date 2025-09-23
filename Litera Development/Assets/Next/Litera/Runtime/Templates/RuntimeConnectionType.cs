using Next.Litera.BuiltIn.RuntimeElements;
using UnityEngine;

namespace Next.Litera.Scripting
{
    [System.Serializable]
    public abstract class RuntimeConnectionType : ScriptableObject
    {
        public abstract void Execute(RuntimeConnection connection);
    }
}