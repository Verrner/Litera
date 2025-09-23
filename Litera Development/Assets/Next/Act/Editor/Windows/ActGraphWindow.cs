using Next.Litera;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Next.Act
{
    [GraphWindowInfo(typeof(ActExecutionProject), "start-based-entries", "act-addon")]
    public sealed class ActGraphWindow : GraphWindow
    {
        [MenuItem("Act/Windows/Act Graph #a")]
        public static ActGraphWindow ShowWindow()
        {
            ActGraphWindow window = CreateInstance<ActGraphWindow>();
            window.Show();
            return window;
        }

        protected override void OnSetuped()
        {
            RegisterWindowInstanceUnique(this);
            Title = "Act Graph";
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            Object obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj is ActExecutionProject project)
            {
                ActGraphWindow window = ShowWindow();
                window.Load(project);
                return true;
            }
            return false;
        }
    }
}