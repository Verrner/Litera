using Next.Litera.Scripting;
using System.Collections.Generic;
using UnityEngine;

namespace Next.Litera.BuiltIn.RuntimeElements
{
    public sealed class RuntimeConnection : RuntimeGraphElement
    {
        [SerializeField] public ScriptableObject Input { get; set; }
        [SerializeField] public ScriptableObject Output {get;set;}

        [SerializeField] public Vector2 PositionA;
        [SerializeField] public Vector2 PositionB;

        [SerializeField] public List<RuntimeConnectionType> ConnectionTypes = new();

        [SerializeField] public Color AccentColor;

        public void ExecuteTypes() => ConnectionTypes.ForEach(t => t.Execute(this));
    }
}