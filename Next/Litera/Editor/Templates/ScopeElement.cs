using Next.Litera.BuiltIn.RuntimeElements;
using Next.Litera.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Next.Litera
{
    [ElementSerialization(typeof(RuntimeScope), 1000)]
    public abstract class ScopeElement : GraphElement, ICustomElementAdding, IConditionalScopeAddable
    {
        private List<GraphElement> m_children = new();

        public List<GraphElement> ScopeChildren => new(m_children);

        private Color m_color;

        public Color Color
        {
            get => m_color;
            set
            {
                m_color = value;
                ColorChanged?.Invoke();
                Dirty = true;
            }
        }

        public Action ColorChanged { get; set; }
        public Action ScopeSizeRefreshed { get; set; }
        public Action ChildrenListChanged { get; set; }

        protected VisualElement ScopeContainer { get; private set; }
        protected VisualElement ScopeChildrenContentContainer { get; private set; }

        public ScopeElement(GraphWindow window) : base(window)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/Graph Scope Styles"));

            ScopeContainer = new();
            ScopeContainer.AddToClassList("scope-container");
            ElementContent.Add(ScopeContainer);

            ColorChanged += RefreshContainerColor;

            ScopeChildrenContentContainer = new();
            ScopeChildrenContentContainer.AddToClassList("scope-children-container");
            ScopeChildrenContentContainer.AddManipulator(new ScopeChildrenContentContainer(this, WindowInstance));
            ScopeContainer.Add(ScopeChildrenContentContainer);

            ColorChanged += RefreshChildrenContentContainer;

            BeforeDeletion += () =>
            {
                while (m_children.Count > 0) RemoveChild(m_children[0]);
            };

            m_color = new(221f / 255f, 221f / 255f, 221f / 255f);
            ColorChanged?.Invoke();
        }

        public void AddChildren(IEnumerable<GraphElement> children)
        {
            foreach (GraphElement e in children)
            {
                if (m_children.Contains(e) || (e is IConditionalScopeAddable addable && !addable.ScopeAddable(this))) continue;

                e.PositionChanged += RefreshContentSize;
                e.SizeChanged += RefreshContentSize;
                m_children.Add(e);
                e.RegisterAddingToScope(this);
            }

            RefreshContentSize();

            ChildrenListChanged?.Invoke();

            Dirty = true;
        }

        public void RemoveChild(GraphElement child)
        {
            if (!m_children.Contains(child)) return;

            m_children.Remove(child);

            child.PositionChanged -= RefreshContentSize;
            child.SizeChanged -= RefreshContentSize;
            child.RegisterRemovingFromScope(this);

            RefreshContentSize();

            ChildrenListChanged?.Invoke();

            Dirty = true;
        }

        public bool ContainsChild(GraphElement child) => m_children.Contains(child);

        protected void RefreshContentSize()
        {
            Rect rect = GetElementsRect(m_children);

            ScopeChildrenContentContainer.style.width = Mathf.Max(100, rect.width + 20);
            ScopeChildrenContentContainer.style.height = Mathf.Max(100, rect.height + 20);

            Position = new(rect.x - 20 - ScopeChildrenContentContainer.localBound.x, rect.y - 20 - ScopeChildrenContentContainer.localBound.y);

            ScopeSizeRefreshed?.Invoke();
        }

        private void RefreshContainerColor()
        {
            ScopeContainer.style.backgroundColor = new Color(Color.r, Color.g, Color.b, .3f);
            ScopeContainer.style.borderRightColor = Color;
            ScopeContainer.style.borderLeftColor = Color;
            ScopeContainer.style.borderBottomColor = Color;
            ScopeContainer.style.borderTopColor = Color;
        }

        private void RefreshChildrenContentContainer()
        {
            Color color = new Color(Color.r, Color.g, Color.b, .1f);
            ScopeChildrenContentContainer.style.backgroundColor = color;
            ScopeChildrenContentContainer.style.borderRightColor = color;
            ScopeChildrenContentContainer.style.borderLeftColor = color;
            ScopeChildrenContentContainer.style.borderBottomColor = color;
            ScopeChildrenContentContainer.style.borderTopColor = color;
            ScopeChildrenContentContainer.style.borderRightWidth = 0;
            ScopeChildrenContentContainer.style.borderLeftWidth = 0;
            ScopeChildrenContentContainer.style.borderBottomWidth = 0;
            ScopeChildrenContentContainer.style.borderTopWidth = 0;
        }

        public override void BeginMovement()
        {
            base.BeginMovement();
            foreach (GraphElement element in m_children)
            {
                if (element.Selected) continue;
                element.BeginMovement();
            }
        }

        public override void FinishMovement()
        {
            base.FinishMovement();
            foreach (GraphElement element in m_children)
            {
                if (element.Selected) continue;
                element.FinishMovement();
            }
        }

        public override void MoveElement(Vector2 deltaFromStart)
        {
            base.MoveElement(deltaFromStart);
            foreach (GraphElement element in m_children)
            {
                if (element.Selected) continue;
                element.MoveElement(deltaFromStart);
            }
        }

        public void CustomAdding(GraphWindow window, List<GraphElement> elements, VisualElement resizableElement)
        {
            elements.Insert(0, this);
            resizableElement.Insert(0, this);
            if (window.Selection.Count > 0) AddChildren(window.Selection);
        }

        protected Rect GetElementsRect(IEnumerable<GraphElement> elements)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (GraphElement element in elements)
            {
                if (element.Position.x < minX) minX = element.Position.x;
                if (element.Position.x + element.resolvedStyle.width > maxX) maxX = element.Position.x + element.resolvedStyle.width;
                if (element.Position.y < minY) minY = element.Position.y;
                if (element.Position.y + element.resolvedStyle.height > maxY) maxY = element.Position.y + element.resolvedStyle.height;
            }

            return new(minX, minY, maxX - minX, maxY - minY);
        }

        public override void DrawInspector(Foldout elementFoldout)
        {
            DrawHeader(elementFoldout);
            DrawColorField(elementFoldout);
            DrawElementsInspector(elementFoldout);
        }

        protected void DrawColorField(Foldout elementFoldout)
        {
            VisualElement parent = new();
            parent.style.flexDirection = FlexDirection.Row;
            parent.style.justifyContent = Justify.SpaceBetween;

            ColorField field = new("Accent Color");
            field.value = Color;
            field.RegisterValueChangedCallback(e =>
            {
                Color = e.newValue;
            });
            field.style.flexGrow = 1;
            parent.Add(field);

            Button reset = new(() =>
            {
                Color = new(221f / 255f, 221f / 255f, 221f / 255f);
                field.value = Color;
            });
            reset.style.width = 20;
            reset.style.height = 20;
            reset.text = "R";
            reset.tooltip = "Reset color.";
            parent.Add(reset);

            elementFoldout.Add(parent);
        }

        protected void DrawElementsInspector(Foldout elementFoldout)
        {
            if (m_children.Count == 0)
            {
                elementFoldout.Add(new HelpBox("This is empty group.", HelpBoxMessageType.Info));
                return;
            }
            Foldout foldout = new() { text = "Elements" };
            elementFoldout.Add(foldout);

            for (int i = 0; i < m_children.Count; i++)
            {
                GraphElement element = m_children[i];

                VisualElement parent = new();
                parent.style.flexDirection = FlexDirection.Row;
                parent.style.justifyContent = Justify.SpaceBetween;

                Label label = new($"{i + 1}. {element}");
                label.style.flexGrow = 1;
                parent.Add(label);

                Button selectionManipulation = new();
                selectionManipulation.clicked += () =>
                {
                    if (element.Selected)
                    {
                        WindowInstance.Deselect(element);
                        selectionManipulation.text = "Select";
                    }
                    else
                    {
                        WindowInstance.Select(element);
                        selectionManipulation.text = "Deselect";
                    }
                };
                selectionManipulation.text = element.Selected ? "Deselect" : "Select";
                parent.Add(selectionManipulation);

                Button focus = new(() => WindowInstance.FocusOnElements(new GraphElement[] { element }));
                focus.text = "Focus";
                parent.Add(focus);
                foldout.Add(parent);
            }
        }

        protected override void OnAfterSerialization(RuntimeGraphElement runtime, List<RuntimeGraphElement> savedRuntimeElements, List<GraphElement> savedGraphElements, string savePath)
        {
            RuntimeScope scope = runtime as RuntimeScope;
            scope.AccentColor = m_color;
            foreach (GraphElement element in m_children)
            {
                int index = savedGraphElements.IndexOf(element);
                if (index == -1) throw new Exception($"Element {element} is not serialized.");
                scope.Children.Add(savedRuntimeElements[index]);
            }
        }

        protected override void OnAfterDeserialization(RuntimeGraphElement element, List<RuntimeGraphElement> loadedRuntimeElements, List<GraphElement> loadedGraphElements, GraphWindow window)
        {
            RuntimeScope scope = element as RuntimeScope;
            AddChildren(scope.Children.Select(r => loadedGraphElements[loadedRuntimeElements.IndexOf(r)]));
            Color = scope.AccentColor;
        }

        public bool ScopeAddable(ScopeElement element) => false;
    }
}