using Next.Litera.Scripting;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Next.Litera
{
    public sealed class GraphElementsInspector : EditorWindow
    {
        private Dictionary<GraphElement, Foldout> m_inspectorFoldouts = new();

        private ScrollView m_elementsScroll;

        private GraphWindow m_graphWindow;

        public GraphWindow GraphWindow
        {
            get => m_graphWindow;
            set
            {
                if (m_graphWindow != null)
                {
                    m_graphWindow.AttachedInspector = null;
                    m_graphWindow.SelectionChanged -= RefreshInspector;
                }

                m_graphWindow = value;
                if (m_graphWindow != null)
                {
                    m_graphWindow.AttachedInspector = this;
                    m_graphWindow.SelectionChanged += RefreshInspector;
                }

                RefreshInspector();
            }
        }

        private static List<GraphElementsInspector> m_instances = new();

        [MenuItem("Litera/Inspector #i")]
        public static void ShowWindow()
        {
            GraphElementsInspector window = CreateInstance<GraphElementsInspector>();
            window.titleContent.text = "Litera Inspector";
            window.Show();
        }

        private void OnEnable()
        {
            m_instances.Add(this);
            Setup();
        }

        private void OnDisable()
        {
            m_instances.Remove(this);
            if (GraphWindow != null) GraphWindow.SelectionChanged -= RefreshInspector;
        }

        public static void RefreshAll() => m_instances.ForEach(e => e.Refresh());

        public void Refresh()
        {
            rootVisualElement.Clear();
            SetupWindowsDropdown();
            rootVisualElement.Add(m_elementsScroll);
            RefreshInspector();
        }

        private void Setup()
        {
            SetupWindowsDropdown();

            m_elementsScroll = new(ScrollViewMode.Vertical);
            rootVisualElement.Add(m_elementsScroll);

            SetupStyles();
            RefreshInspector();
        }

        private void SetupWindowsDropdown()
        {
            List<string> windowsNames = GraphWindow.GetAllWindowsKeys();

            if (windowsNames.Count == 0)
            {
                rootVisualElement.Add(new HelpBox($"There are no any registered graph windows.", HelpBoxMessageType.Info));
                return;
            }

            windowsNames.Reverse();

            if (GraphWindow == null)
            {
                m_graphWindow = GraphWindow.GetWindowInstance(windowsNames[0]);
                m_graphWindow.AttachedInspector = this;
                m_graphWindow.SelectionChanged += RefreshInspector;
            }

            DropdownField windowsDropdown = new("Graph Window", windowsNames, windowsNames.IndexOf(GraphWindow.WindowName));
            windowsDropdown.RegisterValueChangedCallback(e =>
            {
                GraphWindow = GraphWindow.GetWindowInstance(e.newValue);
            });
            windowsDropdown.style.marginBottom = 10;
            rootVisualElement.Add(windowsDropdown);
        }

        private void SetupStyles()
        {
            rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("Styles/Graph Elements Inspector Styles"));
            rootVisualElement.AddToClassList("root-visual-element");
        }

        public void RefreshInspector()
        {
            m_elementsScroll.Clear();
            m_inspectorFoldouts.Clear();

            if (GraphWindow == null) return;

            foreach (GraphElement element in GraphWindow.Selection)
            {
                Foldout foldout = new();
                foldout.text = $"{element.Title} ({element.GetType().Name})";
                foldout.RegisterCallback<PointerDownEvent>(e =>
                {
                    if (e.button == 1)
                    {
                        GenericMenu menu = new();
                        menu.AddItem(new("Deselect"), false, () => GraphWindow.Deselect(element));
                        menu.AddItem(new("Focus"), false, () => GraphWindow.FocusOnElements(new GraphElement[] { element }));
                        element.GetInspectorFoldoutGeneric(menu);
                        menu.ShowAsContext();
                    }
                });
                foldout.AddToClassList("element-foldout");

                element.DrawInspector(foldout);

                m_elementsScroll.Add(foldout);

                m_inspectorFoldouts.Add(element, foldout);
            }
        }

        public bool TryGetFoldout(GraphElement element, out Foldout foldout) => m_inspectorFoldouts.TryGetValue(element, out foldout);

        public Foldout GetFoldout(GraphElement element) => m_inspectorFoldouts[element];
    }
}