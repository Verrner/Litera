using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Next.Litera.Scripting
{
    public abstract class GraphProject : ScriptableObject
    {
        [SerializeField] public string ProjectName;
        [SerializeField] public List<GraphProjectPatch> Patches = new();

        public abstract ProjectExecutionResult Execute();

        protected GraphProjectPatch GetPatch()
        {
            GraphProjectPatch patch = Patches.Last();
            if (patch == null) throw new Exception($"Project {ProjectName} has no any patches to execute.");
            return patch;
        }

        protected RuntimeGraphElement GetNeccessaryGraphElement(Type type, bool exactType = false)
        {
            RuntimeGraphElement element = GetPatch().Elements.Find(e => e.GetType() == type || (!exactType && e.GetType().IsSubclassOf(type)));
            if (element == null) throw new Exception($"Project {ProjectName} do not have element of type \"{type}\"");
            return element;
        }

        protected T GetNeccessaryGraphElement<T>(bool exactType = false) where T : RuntimeGraphElement => (T)GetNeccessaryGraphElement(typeof(T), exactType);
    }
}