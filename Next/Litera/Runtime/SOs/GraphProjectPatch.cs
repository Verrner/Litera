using Next.Litera.Scripting;
using System.Collections.Generic;
using UnityEngine;

namespace Next.Litera
{
    public class GraphProjectPatch : ScriptableObject
    {
        [SerializeField] public int PatchIndex;
        [SerializeField] public GraphProject Project;
        [SerializeField] public List<RuntimeGraphElement> Elements = new();
    }
}