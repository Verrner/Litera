using Next.Litera.BuiltIn.RuntimeElements;
using Next.Litera.Scripting;
using Next.Litera.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Next.Litera.BuiltIn.OtherGraphElements
{
    [ElementSerialization(typeof(RuntimeConnection), 1000)]
    public sealed class Connection : GraphElement, IConditionalDraggable, IConditionalScopeAddable
    {
        private VisualElement m_contentElement;
        private Image m_contentImage;

        private IConnectionPort m_input;
        private IConnectionPort m_output;

        private List<ConnectionType> m_connectionTypes = new();
        public List<ConnectionType> ConnectionTypes => new(m_connectionTypes);

        private Color m_color;

        public Color Color
        {
            get => m_color;
            set
            {
                m_color = value;
                m_contentImage.style.backgroundColor = m_color;
                ColorChanged?.Invoke();
                Dirty = true;
            }
        }

        public Action ColorChanged { get; set; }

        public IConnectionPort Input
        {
            get => m_input;
            set
            {
                if (m_input != null) m_input.UnregisterCallbacks(this);
                m_input = value;
                m_input.RegisterCallbacks(this);
                RefreshEdges();
            }
        }
        public IConnectionPort Output
        {
            get => m_output;
            set
            {
                if (m_output != null) m_output.UnregisterCallbacks(this);
                m_output = value;
                m_output.RegisterCallbacks(this);
                RefreshEdges();
            }
        }

        public bool Draggable => false;

        public Connection(IConnectionPort input, IConnectionPort output) : this((input as GraphElement).WindowInstance)
        {
            Input = input;
            Output = output;
        }

        public Connection(Vector2 posA, Vector2 posB, IConnectionPort input, IConnectionPort output) : this((input as GraphElement).WindowInstance)
        {
            m_input = input;
            m_output = output;

            input.RegisterCallbacks(this);
            output.RegisterCallbacks(this);

            RefreshEdges(posA, posB);
        }

        public Connection(GraphWindow window) : base(window)
        {
            m_contentElement = new();
            m_contentElement.style.alignSelf = Align.Center;
            m_contentElement.style.justifyContent = Justify.Center;
            ElementContent.Add(m_contentElement);

            m_contentImage = new();
            m_contentImage.style.height = 4;
            m_contentImage.style.alignSelf = Align.Center;
            m_contentElement.Add(m_contentImage);

            BeforeDeletion += () =>
            {
                if (Input != null && Output != null) Output.DisconnectFrom(Input);
                if (Input != null) Input.UnregisterCallbacks(this);
                if (Output != null) Output.UnregisterCallbacks(this);
            };

            Color = new Color(0, 163 / 255f, 1);
        }

        public void RefreshEdges()
        {
            if (Output == null || Input == null) return;

            RefreshEdges(Output.GetOutputPortPosition(), Input.GetInputPortPosition());
        }

        public void RefreshEdges(Vector2 positionA, Vector2 positionB)
        {
            float angle = 0;
            if (positionA.x != positionB.x) angle = Mathf.Rad2Deg * Mathf.Atan((positionA.y - positionB.y) / (positionA.x - positionB.x));
            else if (positionA.y < positionB.y) angle = 90;
            else angle = 270;

            float distance = Vector2.Distance(positionA, positionB);
            m_contentElement.style.width = distance;
            m_contentElement.style.height = distance;
            m_contentImage.style.width = distance;

            Position = positionA - new Vector2((distance - positionB.x + positionA.x) * .5f, positionA.y - positionB.y + (distance - positionA.y + positionB.y) * .5f);

            m_contentImage.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        public override void DrawInspector(Foldout elementFoldout)
        {
            DrawHeader(elementFoldout);

            elementFoldout.text = $"From {(Output as GraphElement).Title} to {(Input as GraphElement).Title}.";

            elementFoldout.Add(new Label($"From {Output} to {Input}."));

            ColorField colorField = new("Accent Color");
            colorField.value = Color;
            colorField.RegisterValueChangedCallback(e =>
            {
                Color = e.newValue;
            });
            elementFoldout.Add(colorField);

            Button changeDirectionButton = new(() =>
            {
                if (Input != null) Input.UnregisterCallbacks(this);
                if (Output != null) Output.UnregisterCallbacks(this);

                IConnectionPort temp = Input;
                Input = Output;
                Output = temp;

                RefreshEdges();

                Dirty = true;

                WindowInstance.AttachedInspector?.RefreshInspector();
            });
            changeDirectionButton.text = "Change Direction";
            elementFoldout.Add(changeDirectionButton);

            Foldout typesFoldout = new() { text = "Connection Types" };
            typesFoldout.style.marginTop = 10;

            Button addButton = new(() =>
            {
                GenericMenu menu = new();
                Type[] types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => t.IsSubclassOf(typeof(ConnectionType)) && !t.IsAbstract && t.GetCustomAttribute<ConnectionTypeAttribute>() != null).ToArray();
                foreach (Type type in types)
                {
                    menu.AddItem(new(type.GetCustomAttribute<ConnectionTypeAttribute>().ConnectionName), false, () =>
                    {
                        ConnectionType connection = (ConnectionType)Activator.CreateInstance(type);
                        connection.ParentConnection = this;
                        m_connectionTypes.Add(connection);
                        Dirty = true;
                        WindowInstance.AttachedInspector?.RefreshInspector();
                    });
                }
                menu.ShowAsContext();
            });
            addButton.text = "Add Connection";
            typesFoldout.Add(addButton);

            if (m_connectionTypes.Count > 0)
            {
                Button clearButton = new(() =>
                {
                    m_connectionTypes.Clear();
                    Dirty = true;
                    WindowInstance.AttachedInspector?.RefreshInspector();
                });
                clearButton.text = "Clear Connections";
                typesFoldout.Add(clearButton);

                foreach (ConnectionType type in m_connectionTypes)
                {
                    Foldout typeFoldout = new();
                    ConnectionTypeAttribute a = type.GetType().GetCustomAttribute<ConnectionTypeAttribute>();

                    Button deleteButton = new(() =>
                    {
                        m_connectionTypes.Remove(type);
                        Dirty = true;
                        WindowInstance.AttachedInspector?.RefreshInspector();
                    });
                    deleteButton.text = "Delete";
                    typeFoldout.Add(deleteButton);

                    int order = m_connectionTypes.IndexOf(type);
                    Button reorderButton = new(() =>
                    {
                        CustomDialogueWindow.Get("Reordering", new(300, 50), w =>
                        {
                            order = EditorGUILayout.IntField("Index", order);
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Confirm") && order < m_connectionTypes.Count - 1)
                            {
                                int oldIndex = m_connectionTypes.IndexOf(type);
                                m_connectionTypes.RemoveAt(oldIndex);
                                m_connectionTypes.Insert(order, type);
                                Dirty = true;
                                WindowInstance.AttachedInspector?.RefreshInspector();
                                w.Close();
                            }
                            if (GUILayout.Button("Cancel")) w.Close();
                            EditorGUILayout.EndHorizontal();
                        });
                    });
                    reorderButton.text = "Reorder";
                    typeFoldout.Add(reorderButton);

                    if (a == null)
                    {
                        typeFoldout.text = "ERROR!";
                        typeFoldout.Add(new HelpBox($"Connection type {type.GetType()} does not marked with {nameof(ConnectionTypeAttribute)}.", HelpBoxMessageType.Error));
                    }
                    else
                    {
                        typeFoldout.text = a.ConnectionName;
                        type.DrawInspector(typeFoldout);
                    }
                    typesFoldout.Add(typeFoldout);
                }
            }

            elementFoldout.Add(typesFoldout);
        }

        public void LoadConnectionTypesFromRuntime(RuntimeConnection runtime)
        {
            List<ConnectionTypeAttribute> attributes = new();
            List<Type> types = new();
            AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => t.IsSubclassOf(typeof(ConnectionType)) && !t.IsAbstract && t.GetCustomAttribute<ConnectionTypeAttribute>() != null).ToList().ForEach(t =>
            {
                attributes.Add(t.GetCustomAttribute<ConnectionTypeAttribute>());
                types.Add(t);
            });
            foreach (RuntimeConnectionType type in runtime.ConnectionTypes)
            {
                Type runtimeType = type.GetType();

                int index = attributes.FindIndex(a => a.RuntimeConnectionType == runtimeType);
                if (index == -1)
                {
                    Debug.LogError($"Editor implementation of {runtimeType} is not found.");
                    continue;
                }

                Type editorType = types[index];
                ConnectionType editor = (ConnectionType)Activator.CreateInstance(editorType);

                Dictionary<string, object> values = new();
                foreach (MemberInfo member in runtimeType.GetMembers())
                {
                    RuntimeDataSourceAttribute a = member.GetCustomAttribute<RuntimeDataSourceAttribute>();
                    if (a == null) continue;

                    values.Add(a.Key, LiteraRuntimeUtilities.GetMemberInfo(member, type));
                }
                foreach (MemberInfo member in editorType.GetMembers())
                {
                    EditorDataSourceAttribute a = member.GetCustomAttribute<EditorDataSourceAttribute>();
                    if (a == null) continue;

                    if (values.TryGetValue(a.Key, out object value))
                    {
                        try
                        {
                            LiteraRuntimeUtilities.SetMemberInfo(member, editor, value);
                        }
                        catch
                        {
                            Debug.LogError($"Can not parse runtime data source to editor data source with key \"{a.Key}\".");
                        }
                    }
                    else Debug.LogError($"Runtime data source with key \"{a.Key}\" is not found.");
                }

                m_connectionTypes.Add(editor);
            }
        }

        public override RuntimeGraphElement Save(List<RuntimeGraphElement> savedRuntimeElements, List<GraphElement> savedGraphElements, string savePath)
        {
            GraphElement outputGraphElement = Output as GraphElement;
            GraphElement inputGraphElement = Input as GraphElement;

            RuntimeConnection runtime = ScriptableObject.CreateInstance<RuntimeConnection>();
            runtime.Title = $"Connection from {outputGraphElement.Title} to {inputGraphElement.Title}";
            runtime.Position = Position;
            runtime.SerializationOrder = -1000;
            runtime.PositionA = Output.GetOutputPortPosition();
            runtime.PositionB = Input.GetInputPortPosition();
            runtime.AccentColor = Color;

            try
            {
                IRuntimeConnectionPort inputNode = savedRuntimeElements[savedGraphElements.IndexOf(inputGraphElement)] as IRuntimeConnectionPort;
                IRuntimeConnectionPort outputNode = savedRuntimeElements[savedGraphElements.IndexOf(outputGraphElement)] as IRuntimeConnectionPort;

                if (inputNode == null || outputNode == null) throw new Exception($"One of connected ports' runtime implementation is not derived from {nameof(IRuntimeConnectionPort)}.");

                runtime.Input = inputNode as ScriptableObject;
                runtime.Output = outputNode as ScriptableObject;
                inputNode.InputConnections.Add(runtime);
                outputNode.OutputConnections.Add(runtime);
            }
            catch
            {
                throw new Exception($"Connection not found. Make sure that all ports has it's runtime version and has serialization order below 1000.");
            }

            foreach (ConnectionType type in m_connectionTypes)
            {
                Type editorType = type.GetType();
                ConnectionTypeAttribute a = editorType.GetCustomAttribute<ConnectionTypeAttribute>();
                if (a == null) continue;
                if (!a.RuntimeConnectionType.IsSubclassOf(typeof(RuntimeConnectionType)) || a.RuntimeConnectionType.IsAbstract)
                {
                    Debug.LogError($"Connection type {editorType} runtime connection type is not inherited from {nameof(RuntimeConnectionType)} or is abstract.");
                    continue;
                }

                RuntimeConnectionType runtimeConnection = (RuntimeConnectionType)ScriptableObject.CreateInstance(a.RuntimeConnectionType);

                Dictionary<string, object> values = new();
                foreach (MemberInfo member in editorType.GetMembers())
                {
                    EditorDataSourceAttribute at = member.GetCustomAttribute<EditorDataSourceAttribute>();
                    if (at == null) continue;

                    values.Add(at.Key, LiteraRuntimeUtilities.GetMemberInfo(member, type));
                }
                foreach (MemberInfo member in a.RuntimeConnectionType.GetMembers())
                {
                    RuntimeDataSourceAttribute at = member.GetCustomAttribute<RuntimeDataSourceAttribute>();
                    if (at == null) continue;

                    if (values.TryGetValue(at.Key, out object value))
                    {
                        try
                        {
                            LiteraRuntimeUtilities.SetMemberInfo(member, runtimeConnection, value);
                        }
                        catch
                        {
                            Debug.LogError($"Can not parse editor data source to runtime data source with key \"{at.Key}\".");
                        }
                    }
                    else Debug.LogError($"Editor data source with key \"{at.Key}\" is not found.");
                }

                AssetDatabase.CreateAsset(runtimeConnection, AssetDatabase.GenerateUniqueAssetPath($"{savePath}/Runtime Connection Types/{editorType} - From {outputGraphElement.Title} to {inputGraphElement.Title}.asset"));
                AssetDatabase.SaveAssetIfDirty(runtimeConnection);
                runtime.ConnectionTypes.Add(runtimeConnection);
            }

            return runtime;
        }

        public override void Load(RuntimeGraphElement element, List<RuntimeGraphElement> loadedRuntimeElements, List<GraphElement> loadedGraphElements, GraphWindow window)
        {
            Dirty = false;
            WindowInstance.RemoveElement(this);
        }

        public bool ScopeAddable(ScopeElement element) => false;
    }
}