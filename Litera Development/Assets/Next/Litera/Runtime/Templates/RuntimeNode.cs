using Next.Litera.BuiltIn.RuntimeElements;
using System.Collections.Generic;
using UnityEngine;

namespace Next.Litera.Scripting
{
    public abstract class RuntimeNode : RuntimeGraphElement, IRuntimeConnectionPort
    {
        [SerializeField] private List<RuntimeConnection> m_inputConnections = new();
        [SerializeField] private List<RuntimeConnection> m_outputConnections = new();

        List<RuntimeConnection> IRuntimeConnectionPort.InputConnections { get => m_inputConnections; set => m_inputConnections = value; }
        List<RuntimeConnection> IRuntimeConnectionPort.OutputConnections { get => m_outputConnections; set => m_outputConnections = value; }

        public abstract NodeExecutionResult Execute(GraphProject project);
    }
}