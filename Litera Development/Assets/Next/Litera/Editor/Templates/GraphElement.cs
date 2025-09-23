using Next.Litera.BuiltIn.OtherGraphElements;
using Next.Litera.BuiltIn.RuntimeElements;
using Next.Litera.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Next.Litera.Scripting
{
    public abstract class GraphElement : VisualElement
    {
        private Vector2 m_position;

        [EditorDataSource("position")] public Vector2 Position
        {
            get => m_position;
            set
            {
                m_position = value;
                transform.position = new(m_position.x + WindowInstance.CameraPosition.x, m_position.y + WindowInstance.CameraPosition.y);
                PositionChanged?.Invoke();
                Dirty = true;
            }
        }

        private string m_title;

        [EditorDataSource("title")] public virtual string Title
        {
            get => m_title;
            set
            {
                m_title = value;
                TitleChanged?.Invoke();
                Dirty = true;
            }
        }

        [EditorDataSource("enable-sprite")] public bool EnableSprite { get; set; } = true;

        private Sprite m_sprite;

        [EditorDataSource("sprite")] public Sprite Sprite
        {
            get => m_sprite;
            set
            {
                m_sprite = value;
                if (value == null && LODSpriteImage != null)
                {
                    LODContainer.Remove(LODSpriteImage);
                    LODSpriteImage = null;
                }
                else if (value != null && LODSpriteImage == null)
                {
                    LODSpriteImage = new();
                    LODSpriteImage.AddToClassList("lod-sprite-image");
                    LODContainer.Insert(0, LODSpriteImage);
                }
                if (value != null) LODSpriteImage.sprite = value;
            }
        }

        private float m_spriteSize = 1f;

        [EditorDataSource("sprite-size")] public float SpriteSize
        {
            get => m_spriteSize;
            set
            {
                m_spriteSize = value;
                if (LODSpriteImage != null)
                {
                    LODSpriteImage.style.height = value * WindowInstance.BaseImageSizeMultipler;
                    LODSpriteImage.style.width = value * WindowInstance.BaseImageSizeMultipler;
                }
            }
        }

        protected VisualElement LODContainer { get; private set; }
        protected Image LODSpriteImage { get; private set; }

        public GraphWindow WindowInstance { get; private set; }

        private List<ScopeElement> m_scopes = new();
        public List<ScopeElement> Scopes => new(m_scopes);
        public bool InScope => Scopes.Count > 0;

        public Action TitleChanged { get; set; }
        public Action PositionChanged { get; set; }
        public Action SelectionChanged { get; set; }

        public bool Selected => WindowInstance.Selection.Contains(this);

        public VisualElement ElementContent { get; private set; }

        private bool m_dirty = false;
        public bool Dirty
        {
            get => m_dirty;
            set
            {
                if (value && WindowInstance.Loading) return;
                m_dirty = value;
                if (m_dirty) WindowInstance.Dirty = true;
            }
        }

        private Vector2 m_startMovementPosition;
        private Vector2Field m_positionField;

        public Action AfterAddition { get; set; }
        public Action BeforeDeletion { get; set; }
        public Action SizeChanged { get; set; }

        public GraphElement(GraphWindow window)
        {
            WindowInstance = window;

            style.position = UnityEngine.UIElements.Position.Absolute;

            AddToClassList("collider-element");

            styleSheets.Add(Resources.Load<StyleSheet>("Styles/Graph Element Styles"));

            ElementContent = new();
            Add(ElementContent);

            style.height = new(StyleKeyword.Auto);
            style.width = new(StyleKeyword.Auto);
            ElementContent.style.height = new(StyleKeyword.Auto);
            ElementContent.style.width = new(StyleKeyword.Auto);
            ElementContent.style.overflow = Overflow.Hidden;

            Dirty = false;

            PositionChanged += () => { if (m_positionField != null) m_positionField.value = Position; };

            BeforeDeletion += () =>
            {
                while (m_scopes.Count > 0) m_scopes[0].RemoveChild(this);
            };

            RegisterCallback<GeometryChangedEvent>(e => SizeChanged?.Invoke());

            LODContainer = new();
            LODContainer.visible = false;
            LODContainer.style.position = UnityEngine.UIElements.Position.Absolute;
            WindowInstance.ContentContainer.Add(LODContainer);
            WindowInstance.CameraPositionChanged += RefreshLODContainerPosition;
            WindowInstance.ZoomChanged += RefreshLODContainerPosition;

            PalletteInfoAttribute a = GetType().GetCustomAttribute<PalletteInfoAttribute>();
            if (a != null) Sprite = a.Sprite;

            SpriteSize = 1;
        }

        private void RefreshLODContainerPosition()
        {
            LODContainer.transform.position = WindowInstance.GridToLocalPosition(Position + new Vector2(resolvedStyle.width / 2f, resolvedStyle.height / 2f)) - new Vector2(LODContainer.resolvedStyle.width / 2f, LODContainer.resolvedStyle.height / 2f);
        }

        public void Select() => WindowInstance.Select(this);

        public void Deselect() => WindowInstance.Deselect(this);

        public void RegisterAddingToScope(ScopeElement scope)
        {
            if (m_scopes.Contains(scope)) throw new Exception($"Element {this} already in scope {scope}.");

            m_scopes.Add(scope);
        }

        public void RegisterRemovingFromScope(ScopeElement scope)
        {
            if (!m_scopes.Contains(scope)) throw new Exception($"Scope {scope} does not contain element {this}.");

            m_scopes.Remove(scope);
        }

        public bool IsInScope(ScopeElement scope) => m_scopes.Contains(scope);

        public void RedrawSelection()
        {
            ToggleInClassList("collider-element-selected");
            SelectionChanged?.Invoke();

            OnSelectionChanged();
        }

        protected virtual void OnSelectionChanged()
        {

        }

        public virtual void MoveElement(Vector2 deltaFromStart)
        {
            Position = deltaFromStart + m_startMovementPosition;
        }

        public virtual void BeginMovement()
        {
            m_startMovementPosition = Position;
        }

        public virtual void FinishMovement()
        {

        }

        public virtual void GetGenericMenu(GenericMenu menu) => GetBasicGenericMenu(menu);

        protected void GetBasicGenericMenu(GenericMenu menu)
        {
            menu.AddItem(new("Delete"), false, () => WindowInstance.RemoveElement(this));
        }

        public virtual void DrawInspector(Foldout elementFoldout)
        {
            DrawTitleField(elementFoldout);
            DrawHeader(elementFoldout);
            DrawPositionField(elementFoldout);
        }

        public virtual void GetInspectorFoldoutGeneric(GenericMenu menu)
        {

        }

        protected void DrawPositionField(Foldout element)
        {
            m_positionField = new("Position");
            m_positionField.value = Position;
            m_positionField.RegisterValueChangedCallback(e =>
            {
                Position = e.newValue;
            });
            m_positionField.name = "position-field";
            element.Add(m_positionField);
        }

        protected void DrawTitleField(Foldout element)
        {
            TextField titleField = new();
            titleField.value = Title;
            titleField.RegisterValueChangedCallback(e =>
            {
                Title = e.newValue;
            });
            titleField.name = "title-field";
            titleField.AddToClassList("title-field");
            element.Add(titleField);
        }

        protected void DrawHeader(Foldout element)
        {
            Toggle spriteEnabledToggle = new("Enable Sprite");
            spriteEnabledToggle.tooltip = $"If true, setted sprite will be displayed when zoom reaches {WindowInstance.DisapearingPercent}%.";
            spriteEnabledToggle.value = EnableSprite;
            spriteEnabledToggle.RegisterValueChangedCallback(e => EnableSprite = e.newValue);
            element.Add(spriteEnabledToggle);

            Foldout spriteSettingsFoldout = new() { text = "Sprite Settings" };
            spriteSettingsFoldout.SetEnabled(EnableSprite);
            spriteSettingsFoldout.style.marginBottom = 10;
            element.Add(spriteSettingsFoldout);

            ObjectField spriteField = new("Sprite");
            spriteField.value = m_sprite;
            spriteField.objectType = typeof(Sprite);
            spriteField.RegisterValueChangedCallback(e => Sprite = (Sprite)e.newValue);
            spriteSettingsFoldout.Add(spriteField);

            FloatField spriteSizeField = new("Sprite Size");
            spriteSizeField.value = m_spriteSize;
            spriteSizeField.RegisterValueChangedCallback(e => SpriteSize = e.newValue);
            spriteSettingsFoldout.Add(spriteSizeField);
        }

        public virtual RuntimeGraphElement Save(List<RuntimeGraphElement> savedRuntimeElements, List<GraphElement> savedGraphElements, string savePath)
        {
            OnBeforeSerialization(savedRuntimeElements, savedGraphElements, savePath);

            Type type = GetType();
            ElementSerializationAttribute attribute = type.GetCustomAttribute<ElementSerializationAttribute>();
            if (attribute == null) return null;

            RuntimeGraphElement element = (RuntimeGraphElement)ScriptableObject.CreateInstance(attribute.RuntimeTargetType);

            element.SerializationOrder = attribute.SerializationOrder;

            Dictionary<string, object> values = new();
            foreach (MemberInfo member in type.GetMembers())
            {
                EditorDataSourceAttribute a = member.GetCustomAttribute<EditorDataSourceAttribute>();
                if (a == null) continue;

                values.Add(a.Key, LiteraRuntimeUtilities.GetMemberInfo(member, this));
            }
            foreach (MemberInfo member in attribute.RuntimeTargetType.GetMembers())
            {
                RuntimeDataSourceAttribute a = member.GetCustomAttribute<RuntimeDataSourceAttribute>();
                if (a == null) continue;

                if (values.TryGetValue(a.Key, out object value))
                {
                    try
                    {
                        LiteraRuntimeUtilities.SetMemberInfo(member, element, value);
                    }
                    catch
                    {
                        throw new Exception($"Members with key \"{a.Key}\" can not be casted.");
                    }
                }
                else throw new Exception($"Graph Element {type} does not contain output with key \"{a.Key}\".");
            }

            Dirty = false;

            OnAfterSerialization(element, savedRuntimeElements, savedGraphElements, savePath);

            return element;
        }

        protected virtual void OnBeforeSerialization(List<RuntimeGraphElement> savedRuntimeElements, List<GraphElement> savedGraphElements, string savePath)
        {

        }

        protected virtual void OnAfterSerialization(RuntimeGraphElement runtime, List<RuntimeGraphElement> savedRuntimeElements, List<GraphElement> savedGraphElements, string savePath)
        {

        }

        public virtual void Load(RuntimeGraphElement element, List<RuntimeGraphElement> loadedRuntimeElements, List<GraphElement> loadedGraphElements, GraphWindow window)
        {
            OnBeforeDeserialization(element, loadedRuntimeElements, loadedGraphElements, window);

            Type elementType = element.GetType();
            Type type = GetType();

            Dictionary<string, object> values = new();
            foreach (MemberInfo member in elementType.GetMembers())
            {
                RuntimeDataSourceAttribute a = member.GetCustomAttribute<RuntimeDataSourceAttribute>();
                if (a == null) continue;

                values.Add(a.Key, LiteraRuntimeUtilities.GetMemberInfo(member, element));
            }
            foreach (MemberInfo member in type.GetMembers())
            {
                EditorDataSourceAttribute a = member.GetCustomAttribute<EditorDataSourceAttribute>();
                if (a == null) continue;

                if (values.TryGetValue(a.Key, out object value))
                {
                    try
                    {
                        LiteraRuntimeUtilities.SetMemberInfo(member, this, value);
                    }
                    catch
                    {
                        throw new Exception($"Members with key \"{a.Key}\" can not be casted.");
                    }
                }
                else throw new Exception($"Runtime element {elementType} does not contain output with key \"{a.Key}\".");
            }

            Title = m_title;
            Position = m_position;

            if (this is IConnectionPort connectionPort)
            {
                if (element is not IRuntimeConnectionPort runtimeConnectionPort) throw new Exception($"Runtime type of port {GetType()} must be inherited from {nameof(IRuntimeConnectionPort)}");

                foreach (RuntimeConnection connection in loadedRuntimeElements.Where(r => r is RuntimeConnection conn && conn.Output as IRuntimeConnectionPort == runtimeConnectionPort))
                {
                    int inputIndex = loadedRuntimeElements.IndexOf(connection.Input as RuntimeGraphElement);
                    if (inputIndex == -1) continue;

                    IConnectionPort connectedPort = loadedGraphElements[inputIndex] as IConnectionPort;
                    if (connectionPort.ConnectedWith(connectedPort)) continue;

                    Connection conn = new(connection.PositionA, connection.PositionB, connectedPort, connectionPort);
                    conn.Color = connection.AccentColor;

                    connectionPort.AddToOutputConnections(conn);
                    connectedPort.AddToInputConnections(conn);

                    conn.LoadConnectionTypesFromRuntime(connection);

                    window.InsertElement(Mathf.Min(window.Elements.IndexOf(this), window.Elements.IndexOf(connectedPort as GraphElement)), conn);
                }
                foreach (RuntimeConnection connection in loadedRuntimeElements.Where(r => r is RuntimeConnection conn && conn.Input as IRuntimeConnectionPort == runtimeConnectionPort))
                {
                    int outputIndex = loadedRuntimeElements.IndexOf(connection.Output as RuntimeGraphElement);
                    if (outputIndex == -1) continue;

                    IConnectionPort connectedPort = loadedGraphElements[outputIndex] as IConnectionPort;
                    if (connectionPort.ConnectedWith(connectedPort)) continue;

                    Connection conn = new(connection.PositionA, connection.PositionB, connectionPort, connectedPort);
                    conn.Color = connection.AccentColor;

                    connectedPort.AddToOutputConnections(conn);
                    connectionPort.AddToInputConnections(conn);

                    conn.LoadConnectionTypesFromRuntime(connection);

                    window.InsertElement(Mathf.Min(window.Elements.IndexOf(this), window.Elements.IndexOf(connectedPort as GraphElement)), conn);
                }
            }

            OnAfterDeserialization(element, loadedRuntimeElements, loadedGraphElements, window);

            Dirty = false;
        }

        protected virtual void OnBeforeDeserialization(RuntimeGraphElement element, List<RuntimeGraphElement> loadedRuntimeElement, List<GraphElement> loadedGraphElements, GraphWindow window)
        {

        }

        protected virtual void OnAfterDeserialization(RuntimeGraphElement element, List<RuntimeGraphElement> loadedRuntimeElements, List<GraphElement> loadedGraphElements, GraphWindow window)
        {

        }

        public override string ToString()
        {
            return $"({Title} | {Position} | {GetType()})";
        }

        public virtual void RefreshLOD()
        {
            RefreshDisapearingLOD();
        }

        protected void RefreshDisapearingLOD()
        {
            bool needToDisapear = 100 - 100f * (WindowInstance.Zoom - 1f / WindowInstance.MaxZoom) / (1f / WindowInstance.MinZoom - 1f / WindowInstance.MaxZoom) >= WindowInstance.DisapearingPercent;

            visible = !needToDisapear;
            LODContainer.visible = needToDisapear;
        }
    }
}