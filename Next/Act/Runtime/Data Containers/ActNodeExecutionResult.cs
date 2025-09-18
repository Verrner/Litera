using Next.Litera;

namespace Next.Act
{
    public class ActNodeExecutionResult : NodeExecutionResult
    {
        public readonly bool GoToNextNode;
        public readonly int NextNodeIndex;

        public ActNodeExecutionResult(bool goToNextNode = true, int nextNodeIndex = 0)
        {
            GoToNextNode = goToNextNode;
            NextNodeIndex = nextNodeIndex;

        }
    }
}