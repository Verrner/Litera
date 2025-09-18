using Next.Litera.BuiltIn.RuntimeNodes;
using Next.Litera.Scripting;

namespace Next.Litera.BuiltIn.Nodes
{
    [PalletteInfo("Simple Math", "Divide", "", "", "math", "simple-math"), ElementSerialization(typeof(DivideRuntimeNode))]
    public sealed class DivideNode : BaseMathNode
    {
        public DivideNode(GraphWindow window) : base(window)
        {
            Title = "Divide";
        }
    }
}