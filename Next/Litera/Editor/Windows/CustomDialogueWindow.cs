using System;
using UnityEditor;
using UnityEngine;

namespace Next.Litera
{
    public sealed class CustomDialogueWindow : EditorWindow
    {
        private Action<CustomDialogueWindow> m_draw;

        public static void Get(string name, Vector2 size, Action<CustomDialogueWindow> draw)
        {
            CustomDialogueWindow window = ScriptableObject.CreateInstance<CustomDialogueWindow>();
            window.m_draw = draw;
            window.titleContent = new(name);
            window.minSize = size;
            window.maxSize = size;
            window.ShowUtility();
        }

        private void OnGUI()
        {
            m_draw?.Invoke(this);
        }
    }
}