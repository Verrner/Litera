using Next.Litera.BuiltIn.RuntimeNodes;
using Next.Litera.Scripting;

namespace Next.Litera.BuiltIn.Nodes
{
    [PalletteInfo("Simple Math", "Subtract", "", "", "math", "simple-math"), ElementSerialization(typeof(SubtractRuntimeNode))]
    public sealed class SubtractNode : BaseMathNode
    {
        public SubtractNode(GraphWindow window) : base(window)
        {
            Title = "Subtract";
        }
    }
}