using Next.Litera.BuiltIn.RuntimeNodes;
using Next.Litera.Scripting;

namespace Next.Litera.BuiltIn.Nodes
{
    [PalletteInfo("Entries", "Execution Start", "", "Node that can be used for starting execution.", "all-entries", "start-based-entries"), ElementSerialization(typeof(ExecutionStartRuntimeNode))]
    public class ExecutionStartNode : LiteraNode
    {
        public ExecutionStartNode(GraphWindow window) : base(window)
        {
            Title = "Execution Start";
        }
    }
}