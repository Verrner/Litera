using Next.Litera;
using Next.Litera.BuiltIn.RuntimeElements;
using Next.Litera.BuiltIn.RuntimeNodes;
using Next.Litera.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Next.Act
{
    public sealed class ActExecutionProject : GraphProject
    {
        private List<IRuntimeConnectionPort> m_executed = new();

        private RuntimeNode m_currentNode;

        public Action ExecutionEnded { get; set; }

        public override ProjectExecutionResult Execute()
        {
            m_currentNode = GetNeccessaryGraphElement<ExecutionStartRuntimeNode>();
            return new();
        }

        public bool UpdateExecution()
        {
            while (m_currentNode != null)
            {
                NodeExecutionResult result = m_currentNode.Execute(this);
                ActNodeExecutionResult actResult = result as ActNodeExecutionResult;
                if (actResult != null && !actResult.GoToNextNode) return true;

                m_executed.Add(m_currentNode);

                RuntimeConnection nextConnection = GetNextConnection(m_currentNode, actResult == null ? 0 : actResult.NextNodeIndex);

                while (nextConnection != null && nextConnection.Input is not RuntimeNode)
                {
                    m_executed.Add(nextConnection.Input as IRuntimeConnectionPort);
                    RuntimeConnection[] possibleConnections = (nextConnection.Input as IRuntimeConnectionPort).OutputConnections.Where(c => c.ConnectionTypes.Exists(t => t.GetType() == typeof(StorylineRuntimeConnectionType))).ToArray();
                    nextConnection = possibleConnections.Length > 0 ? possibleConnections[0] : null;
                }

                if (nextConnection == null)
                {
                    m_currentNode = null;
                    ExecutionEnded?.Invoke();
                    return false;
                }

                SetNodeAsCurrent(nextConnection.Input as RuntimeNode);
                return true;
            }

            return false;
        }

        private void SetNodeAsCurrent(RuntimeNode node)
        {
            Prepare(node, true);
            m_currentNode = node;
        }

        private void Prepare(IRuntimeConnectionPort port, bool topLayer)
        {
            foreach (RuntimeConnection connection in port.InputConnections)
            {
                if (!m_executed.Contains(connection.Output as IRuntimeConnectionPort)) Prepare(connection.Output as IRuntimeConnectionPort, false);
                connection.ExecuteTypes();
            }
            if (!topLayer)
            {
                if (port is RuntimeNode node) node.Execute(this);
                m_executed.Add(port);
            }
        }

        private RuntimeConnection GetNextConnection(RuntimeNode node, int index)
        {
            if (index <= -1) return null;
            RuntimeConnection[] connections = (node as IRuntimeConnectionPort).OutputConnections.Where(c => c.ConnectionTypes.Exists(c => c.GetType() == typeof(StorylineRuntimeConnectionType))).ToArray();
            return connections.Length > index ? connections[index] : null;
        }
    }
}