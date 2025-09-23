using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Next.Litera
{
    public abstract partial class GraphWindow : EditorWindow
    {

        private static Dictionary<string, GraphWindow> m_instances = new();

        public GraphElementsInspector AttachedInspector { get; set; }
        public GraphPalletteWindow AttachedPallette { get; set; }

        private string m_windowName;
        public string WindowName
        {
            get => m_windowName;
            set
            {
                m_windowName = value;
            }
        }

        public VisualElement ContentContainer { get; private set; }
        public VisualElement ToolbarContainer { get; private set; }

        private VisualElement m_resizableElement;

        private string m_title = "New Window";

        public string Title
        {
            get => m_title;
            set
            {
                m_title = value;
                RefreshTitle();
            }
        }

        private GraphWindowContentContainerManipulator m_contentManipulator;

        private void OnEnable()
        {
            Setup();
        }

        private void OnDisable()
        {
            if (Dirty && EditorUtility.DisplayDialog("Window Dirty", "Project is dirty. Do you want to save project? You can not cancel window closing.", "Save", "Do not save"))
            {
                Save();
            }

            if (m_instances.ContainsKey(WindowName)) m_instances.Remove(WindowName);

            if (AttachedInspector != null)
            {
                AttachedInspector.RefreshInspector();
                SelectionChanged -= AttachedInspector.RefreshInspector;
                AttachedInspector.GraphWindow = null;
            }

            if (AttachedPallette != null) AttachedPallette.GraphWindow = null;

            GraphPalletteWindow.RefreshAll();
            GraphElementsInspector.RefreshAll();
        }

        private void Setup()
        {
            RefreshTitle();

            ContentContainer = new();
            ContentContainer.StretchToParentSize();
            rootVisualElement.Add(ContentContainer);

            m_resizableElement = new();
            m_resizableElement.StretchToParentSize();
            ContentContainer.Add(m_resizableElement);

            ToolbarContainer = new();
            ToolbarContainer.StretchToParentWidth();
            ToolbarContainer.AddToClassList("toolbar-container");
            rootVisualElement.Add(ToolbarContainer);

            if (AttachedInspector != null) SelectionChanged += AttachedInspector.RefreshInspector;

            SetupManipulators();
            SetupStyles();
            SetupToolbar();
            OnSetuped();

            GraphElementsInspector.RefreshAll();
            GraphPalletteWindow.RefreshAll();
        }

        private void SetupManipulators()
        {
            m_contentManipulator = new GraphWindowContentContainerManipulator(this);
            ContentContainer.AddManipulator(m_contentManipulator);
            ContentContainer.AddManipulator(new GraphWindowZoomManipulator(this));
        }

        private void SetupStyles()
        {
            rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("Styles/Graph Window Styles"));
            rootVisualElement.AddToClassList("root-visual-element");
        }

        protected virtual void OnSetuped()
        {

        }
    }
}