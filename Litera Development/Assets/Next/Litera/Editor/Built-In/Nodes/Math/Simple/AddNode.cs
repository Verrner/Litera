using Next.Litera.BuiltIn.RuntimeNodes;
using Next.Litera.Scripting;

namespace Next.Litera.BuiltIn.Nodes
{
    [PalletteInfo("Simple Math", "Add", "", "", "math", "simple-math"), ElementSerialization(typeof(AddRuntimeNode))]
    public sealed class AddNode : BaseMathNode
    {
        public AddNode(GraphWindow window) : base(window)
        {
            Title = "Add";
        }
    }
}