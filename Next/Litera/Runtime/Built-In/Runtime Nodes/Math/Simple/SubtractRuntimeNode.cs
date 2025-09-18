using Next.Litera.Scripting;

namespace Next.Litera.BuiltIn.RuntimeNodes
{
    public sealed class SubtractRuntimeNode : BaseMathRuntimeNode
    {
        public override NodeExecutionResult Execute(GraphProject project)
        {
            result = a - b;
            return new();
        }
    }
}