using UnityEditor;
using UnityEngine;

namespace Next.Act
{
    [CustomEditor(typeof(ActExecuter), true)]
    public sealed class ActExecuterCI : Editor
    {
        private ActExecuter m_target;

        private void OnEnable()
        {
            m_target = (ActExecuter)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            m_target.Project = (ActExecutionProject)EditorGUILayout.ObjectField("Project", m_target.Project, typeof(ActExecutionProject), false);
            
            GUI.enabled = m_target.Project != null;
            m_target.ActivateOnAwake = EditorGUILayout.Toggle("Activate On Awake", m_target.ActivateOnAwake);
            GUI.enabled = true;

            if (m_target.Project == null) EditorGUILayout.HelpBox("Project is null.", MessageType.Warning);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_target);
            }
        }
    }
}