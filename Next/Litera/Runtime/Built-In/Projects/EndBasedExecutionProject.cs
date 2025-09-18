using Next.Litera.BuiltIn.RuntimeElements;
using Next.Litera.BuiltIn.RuntimeNodes;
using Next.Litera.Scripting;
using System.Collections.Generic;

namespace Next.Litera.BuiltIn.Projects
{
    public class EndBasedExecutionProject : GraphProject
    {
        private List<IRuntimeConnectionPort> m_executedPorts = new();

        public override ProjectExecutionResult Execute()
        {
            Execute(GetNeccessaryGraphElement<ExecutionResultRuntimeNode>());

            return new();
        }

        protected virtual void Execute(IRuntimeConnectionPort port)
        {
            foreach (RuntimeConnection connection in port.InputConnections)
            {
                if (m_executedPorts.Contains(connection.Output as IRuntimeConnectionPort)) continue;

                Execute(connection.Output as IRuntimeConnectionPort);
                connection.ExecuteTypes();
            }

            if (port is RuntimeNode node) node.Execute(this);
            m_executedPorts.Add(port);
        }
    }
}