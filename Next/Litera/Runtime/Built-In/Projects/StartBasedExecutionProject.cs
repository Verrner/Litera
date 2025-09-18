using Next.Litera.BuiltIn.RuntimeElements;
using Next.Litera.BuiltIn.RuntimeNodes;
using Next.Litera.Scripting;
using System.Collections.Generic;
using System.Linq;

namespace Next.Litera.BuiltIn.Projects
{
    public class StartBasedExecutionProject : GraphProject
    {
        private List<IRuntimeConnectionPort> m_executedPorts = new();

        public override ProjectExecutionResult Execute()
        {
            Execute(GetNeccessaryGraphElement<ExecutionStartRuntimeNode>());

            return new();
        }

        protected virtual void Execute(IRuntimeConnectionPort port)
        {
            m_executedPorts.Add(port);

            foreach (IRuntimeConnectionPort output in port.InputConnections.Select(c => c.Output))
            {
                if (m_executedPorts.Contains(output)) continue;
                Execute(output);
            }

            foreach (RuntimeConnection connection in port.OutputConnections)
            {
                connection.ExecuteTypes();
                if (m_executedPorts.Contains(connection.Input as IRuntimeConnectionPort)) continue;
                Execute(connection.Input as IRuntimeConnectionPort);
            }
        }
    }
}