using Next.Litera.Scripting;
using System.Collections.Generic;
using UnityEngine;

namespace Next.Litera.BuiltIn.RuntimeElements
{
    public class RuntimeScope : RuntimeGraphElement
    {
        [SerializeField] public List<RuntimeGraphElement> Children = new();
        [SerializeField] public Color AccentColor;
    }
}