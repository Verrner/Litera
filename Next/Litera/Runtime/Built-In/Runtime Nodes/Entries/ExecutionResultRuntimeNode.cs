using Next.Litera.Scripting;
using System;
using UnityEngine;

namespace Next.Litera.BuiltIn.RuntimeNodes
{
    public class ExecutionResultRuntimeNode : RuntimeNode
    {
        public override NodeExecutionResult Execute(GraphProject project)
        {
            Debug.Log($"Execution Finished (Project: {project.ProjectName} | Time: {DateTime.Now})");
            return new();
        }
    }
}