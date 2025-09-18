using Next.Litera.BuiltIn.RuntimeNodes;
using Next.Litera.Scripting;

namespace Next.Litera.BuiltIn.Nodes
{
    [PalletteInfo("Entries", "Execution Result", "", "Node that can be used for receiving result of execution.", "all-entries", "end-based-entries"), ElementSerialization(typeof(ExecutionResultRuntimeNode))]
    public class ExecutionResultNode : LiteraNode
    {
        public ExecutionResultNode(GraphWindow window) : base(window)
        {
            Title = "Execution Result";
        }
    }
}