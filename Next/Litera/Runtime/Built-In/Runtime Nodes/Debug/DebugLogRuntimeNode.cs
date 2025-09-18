using Next.Litera.Scripting;
using UnityEngine;

namespace Next.Litera.BuiltIn.RuntimeNodes
{
    public sealed class DebugLogRuntimeNode : RuntimeNode
    {
        [RuntimeDataSource("type")] public DebugLogType Type;
        [RuntimeDataSource("message"), DataTransportInput("message")] public string Message;

        public override NodeExecutionResult Execute(GraphProject project)
        {
            switch (Type)
            {
                case DebugLogType.Info:
                    Debug.Log(Message);
                    break;
                case DebugLogType.Warning:
                    Debug.LogWarning(Message);
                    break;
                case DebugLogType.Error:
                    Debug.LogError(Message);
                    break;
            }
            return new();
        }
    }
}