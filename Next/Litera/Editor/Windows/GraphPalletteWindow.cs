using Next.Litera.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Next.Litera
{
    public sealed class GraphPalletteWindow : EditorWindow
    {
        private string m_filter = "";

        private class PalletteElement
        {
            public string Name;
            public string GroupName;
            public string Tooltip;
            public Sprite Sprite;
            public Type NodeType;
            public int Order;
            public int GroupOrder;
            public string[] Collections;
            public bool EnableInAllWindows;
        }

        private class GroupInfo
        {
            public int Order;
            public string Name;
            public List<PalletteElement> Elements;
        }

        private GraphWindow m_graphWindow;

        public GraphWindow GraphWindow
        {
            get => m_graphWindow;
            set
            {
                if (m_graphWindow != null) m_graphWindow.AttachedPallette = null;
                m_graphWindow = value;
                if (value != null) m_graphWindow.AttachedPallette = this;
                Refresh();
            }
        }

        private List<PalletteElement> m_elements = new();

        private static List<GraphPalletteWindow> m_instances = new();

        private ScrollView m_elementsScroll;

        [MenuItem("Litera/Graph Pallette #p")]
        public static GraphPalletteWindow ShowWindow()
        {
            GraphPalletteWindow window = CreateInstance<GraphPalletteWindow>();
            window.titleContent.text = "Graph Pallette";
            window.Show();
            return window;
        }

        private void OnEnable()
        {
            m_instances.Add(this);
            SetupStyles();
            Setup();
        }

        private void OnDisable()
        {
            if (GraphWindow != null) GraphWindow.AttachedPallette = null;
            m_instances.Remove(this);
        }

        public static void RefreshAll()
        {
            m_instances.ForEach(e => e.Refresh());
        }

        public void Refresh()
        {
            rootVisualElement.Clear();
            Setup();
        }

        private void Setup()
        {
            SetupWindowsDropdown();
            if (GraphWindow != null)
            {
                SetupElements();
                SetupPalletteLists();
                RedrawPalletteList();
            }
        }

        private void SetupStyles()
        {
            rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("Styles/Graph Pallette Window Styles"));
            rootVisualElement.AddToClassList("root-visual-element");
        }

        private void SetupWindowsDropdown()
        {
            List<string> windowsNames = GraphWindow.GetAllWindowsKeys();

            if (windowsNames.Count == 0)
            {
                rootVisualElement.Add(new HelpBox("There are no any registered graph windows.", HelpBoxMessageType.Info));
                return;
            }

            windowsNames.Reverse();

            if (GraphWindow == null)
            {
                m_graphWindow = GraphWindow.GetWindowInstance(windowsNames[0]);
                m_graphWindow.AttachedPallette = this;
            }

            DropdownField windowsDropdown = new("Graph Window", windowsNames, windowsNames.IndexOf(GraphWindow.WindowName));
            windowsDropdown.RegisterValueChangedCallback(e =>
            {
                GraphWindow = GraphWindow.GetWindowInstance(e.newValue);
            });
            rootVisualElement.Add(windowsDropdown);
        }

        private void SetupElements()
        {
            TextField filterField = new();
            filterField.name = "filter-field";
            filterField.RegisterValueChangedCallback(e =>
            {
                m_filter = e.newValue;
                RedrawPalletteList();
            });
            filterField.AddToClassList("filter-field");
            rootVisualElement.Add(filterField);

            m_elementsScroll = new(ScrollViewMode.Vertical);
            m_elementsScroll.AddToClassList("elements-scroll");
            rootVisualElement.Add(m_elementsScroll);
        }

        private void SetupPalletteLists()
        {
            GraphWindowInfoAttribute attribute = GraphWindow.GetType().GetCustomAttribute<GraphWindowInfoAttribute>();
            bool enableAllCollections = attribute == null || attribute.EnableAllCollections;

            m_elements = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(GraphElement)) && t.GetCustomAttribute<PalletteInfoAttribute>() != null).Select(t =>
            {
                PalletteInfoAttribute attribute = t.GetCustomAttribute<PalletteInfoAttribute>();
                string[] nameParts = attribute.Name.Split('|');
                string[] groupParts = attribute.GroupName.Split('|');
                return new PalletteElement() { GroupName = groupParts[0], GroupOrder = groupParts.Length == 2 ? int.TryParse(groupParts[1], out int go) ? go : 0 : 0, Name = nameParts[0], Order = nameParts.Length == 2 ? int.TryParse(nameParts[1], out int o) ? o : 0 : 0, NodeType = t, Sprite = attribute.Sprite, Tooltip = attribute.Tooltip, Collections = attribute.ElementsCollections, EnableInAllWindows = attribute.EnableInAllWindows };
            }).Where(e =>
            {
                if (enableAllCollections || e.EnableInAllWindows) return true;

                for (int i = 0; i < e.Collections.Length; i++) for (int j = 0; j < attribute.Collections.Length; j++) if (e.Collections[i] == attribute.Collections[j]) return true;

                return false;
            }).ToList();
        }

        private void RedrawPalletteList()
        {
            m_elementsScroll.Clear();

            List<GroupInfo> groups = new();
            m_elements.Where(e =>
            {
                string formattedFilter = m_filter.Replace(" ", "").ToLower();
                return string.IsNullOrWhiteSpace(m_filter) || e.GroupName.Replace(" ", "").ToLower().Contains(formattedFilter) || e.Name.Replace(" ", "").ToLower().Contains(formattedFilter);
            }).ToList().ForEach(e =>
            {
                int index = groups.FindIndex(i => i.Name == e.GroupName);
                if (index == -1) groups.Add(new() { Order = e.GroupOrder, Elements = new() { e }, Name = e.GroupName });
                else
                {
                    groups[index].Elements.Add(e);
                    groups[index].Order = Mathf.Min(groups[index].Order, e.GroupOrder);
                }
            });

            groups.Sort((a, b) => a.Order - b.Order);
            groups.ForEach(g => g.Elements.Sort((a, b) => a.Order - b.Order));

            foreach (GroupInfo group in groups)
            {
                Foldout groupFoldout = new();
                groupFoldout.AddToClassList("group-foldout");
                groupFoldout.value = true;
                groupFoldout.text = group.Name;

                foreach (PalletteElement element in group.Elements)
                {
                    Type type = element.NodeType;
                    Button elementButton = new(() =>
                    {
                        if (GraphWindow != null)
                        {
                            GraphElement graphElement = (GraphElement)Activator.CreateInstance(type, new object[] { GraphWindow });
                            graphElement.Position = GraphWindow.LocalToGridPosition(GraphWindow.position.size / 2f);
                            GraphWindow.AddElement(graphElement);
                            GraphWindow.Select(graphElement);
                        }
                    });
                    elementButton.tooltip = element.Tooltip;
                    elementButton.AddToClassList("element-button");
                    elementButton.Clear();

                    VisualElement elementContentContainer = new();
                    elementContentContainer.AddToClassList("element-content-container");
                    elementButton.Add(elementContentContainer);

                    if (element.Sprite != null)
                    {
                        Image image = new();
                        image.AddToClassList("element-content-image");
                        image.sprite = element.Sprite;
                        elementContentContainer.Add(image);
                    }
                    if (!string.IsNullOrWhiteSpace(element.Name))
                    {
                        Label label = new(element.Name);
                        label.AddToClassList("element-content-label");
                        elementContentContainer.Add(label);
                    }

                    groupFoldout.Add(elementButton);
                }
                m_elementsScroll.Add(groupFoldout);
            }
        }
    }
}