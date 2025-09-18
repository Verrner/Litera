using Next.Litera.Scripting;
using System;
using UnityEngine;

namespace Next.Litera.BuiltIn.RuntimeNodes
{
    public class ExecutionStartRuntimeNode : RuntimeNode
    {
        public override NodeExecutionResult Execute(GraphProject project)
        {
            Debug.Log($"Execution Started (Project: {project.ProjectName} | Time: {DateTime.Now})");
            return new();
        }
    }
}