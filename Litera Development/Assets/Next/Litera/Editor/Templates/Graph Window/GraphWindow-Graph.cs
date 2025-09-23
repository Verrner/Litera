using Next.Litera.BuiltIn.Projects;
using Next.Litera.Scripting;
using Next.Litera.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Next.Litera
{
    public abstract partial class GraphWindow
    {
        private Vector2 m_cameraPosition;

        public Vector2 CameraPosition
        {
            get => m_cameraPosition;
            set
            {
                m_cameraPosition = value;
                m_elements.ForEach(e => e.transform.position = new(m_cameraPosition.x + e.Position.x, m_cameraPosition.y + e.Position.y));
                CameraPositionChanged?.Invoke();
            }
        }

        public Action CameraPositionChanged { get; set; }

        public bool MovingSelection => m_contentManipulator.MovingSelection;

        public float Zoom
        {
            get => m_resizableElement.transform.scale.x;
            set
            {
                float zoom = Mathf.Clamp(value, 1f / MaxZoom, 1f / MinZoom);
                if (m_resizableElement.transform.scale.x != zoom)
                {
                    m_resizableElement.transform.scale = new(zoom, zoom);
                    RefreshLOD();
                }
                ZoomChanged?.Invoke();
            }
        }

        public Action ZoomChanged { get; set; }

        public float MinZoom { get; private set; } = .5f;
        public float MaxZoom { get; private set; } = 20f;

        public float DisapearingPercent { get; private set; } = 75f;
        public float BaseImageSizeMultipler { get; private set; } = 100f;

        private List<GraphElement> m_elements = new();

        public List<GraphElement> Elements => new(m_elements);

        public Action ElementsListChanged { get; set; }
        public Action SelectionMoved { get; set; }

        private GraphProject m_lastOpenedProject;

        public GraphProject LastOpenedProject
        {
            get => m_lastOpenedProject;
            set
            {
                m_lastOpenedProject = value;
                RefreshTitle();
            }
        }

        private bool m_dirty;

        public bool Dirty
        {
            get => m_dirty;
            set
            {
                m_dirty = value;

                RefreshTitle();
            }
        }

        public bool Loading { get; private set; }

        public void AddElement(GraphElement element)
        {
            if (m_elements.Contains(element)) return;

            if (element is not ICustomElementAdding adding)
            {
                m_elements.Add(element);
                m_resizableElement.Add(element);
            }
            else adding.CustomAdding(this, m_elements, m_resizableElement);

            element.AfterAddition?.Invoke();

            ElementsListChanged?.Invoke();

            Dirty = true;
        }

        public void InsertElement(int index, GraphElement element)
        {
            if (m_elements.Contains(element)) return;

            m_elements.Insert(index, element);
            m_resizableElement.Insert(index, element);

            element.AfterAddition?.Invoke();

            ElementsListChanged?.Invoke();

            Dirty = true;
        }

        public void RemoveElement(GraphElement element)
        {
            if (!m_elements.Contains(element)) return;

            element.BeforeDeletion?.Invoke();

            if (m_selection.Contains(element))
            {
                m_selection.Remove(element);
                AttachedInspector?.RefreshInspector();
            }

            if (m_resizableElement.Contains(element)) m_resizableElement.Remove(element);
            m_elements.Remove(element);

            ElementsListChanged?.Invoke();

            Dirty = true;
        }

        public void FocusOnElements(IEnumerable<GraphElement> elements)
        {
            if (elements.Count() == 0) return;

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (GraphElement element in elements)
            {
                if (element.Position.x < minX) minX = element.Position.x;
                if (element.Position.y < minY) minY = element.Position.y;
                if (element.Position.x + element.resolvedStyle.width > maxX) maxX = element.Position.x + element.resolvedStyle.width;
                if (element.Position.y + element.resolvedStyle.height > maxY) maxY = element.Position.y + element.resolvedStyle.height;
            }

            CameraPosition = new Vector2(-minX, -minY) - new Vector2((maxX - minX) / 2f, (maxY - minY) / 2f) + position.size / 2f;

            float xZoom = position.width / (maxX - minX + 100);
            float yZoom = position.height / (maxY - minY + 100);

            Zoom = Mathf.Min(xZoom, yZoom);
        }

        public void RefreshLOD()
        {
            m_elements.ForEach(e => e.RefreshLOD());
        }

        public void Save()
        {
            if (LastOpenedProject == null) SaveAs();
            else
            {
                GraphProjectAttribute projectAttribute = LastOpenedProject.GetType().GetCustomAttribute<GraphProjectAttribute>();
                GraphProjectPatch patch = (GraphProjectPatch)ScriptableObject.CreateInstance(projectAttribute == null || !projectAttribute.PatchType.IsSubclassOf(typeof(GraphProjectPatch)) || projectAttribute.PatchType.IsAbstract ? typeof(GraphProjectPatch) : projectAttribute.PatchType);
                patch.PatchIndex = LastOpenedProject.Patches.Count;
                patch.Project = LastOpenedProject;
                LastOpenedProject.Patches.Add(patch);

                string path = AssetDatabase.GetAssetPath(LastOpenedProject);
                path = path.Remove(path.Length - LastOpenedProject.name.Length - ".asset".Length - 1, LastOpenedProject.name.Length + ".asset".Length + 1);

                AssetDatabase.CreateFolder($"{path}/Patches", $"Patch {patch.PatchIndex}");
                path += $"/Patches/Patch {patch.PatchIndex}";

                AssetDatabase.CreateAsset(patch, $"{path}/Patch {patch.PatchIndex}.asset");

                AssetDatabase.CreateFolder(path, "Elements");
                AssetDatabase.CreateFolder(path, "Runtime Connection Types");

                List<GraphElement> savedGraphElements = new();
                List<RuntimeGraphElement> savedRuntimeElements = new();

                List<GraphElement> sortedElements = new(m_elements);
                sortedElements.Sort((a, b) =>
                {
                    ElementSerializationAttribute attA = a.GetType().GetCustomAttribute<ElementSerializationAttribute>();
                    ElementSerializationAttribute attB = b.GetType().GetCustomAttribute<ElementSerializationAttribute>();
                    return (attA != null ? attA.SerializationOrder : 0) - (attB != null ? attB.SerializationOrder : 0);
                });

                foreach (GraphElement element in sortedElements)
                {
                    RuntimeGraphElement runtime = element.Save(savedRuntimeElements, savedGraphElements, path);
                    if (runtime == null) continue;
                    AssetDatabase.CreateAsset(runtime, AssetDatabase.GenerateUniqueAssetPath($"{path}/Elements/{runtime.Title} {runtime.GetType()}.asset"));

                    patch.Elements.Add(runtime);
                    savedGraphElements.Add(element);
                    savedRuntimeElements.Add(runtime);
                }

                savedRuntimeElements.ForEach(e =>
                {
                    EditorUtility.SetDirty(e);
                    AssetDatabase.SaveAssetIfDirty(e);
                });

                EditorUtility.SetDirty(patch);
                AssetDatabase.SaveAssetIfDirty(patch);

                CustomSave(path, patch);

                EditorUtility.SetDirty(LastOpenedProject);
                AssetDatabase.SaveAssetIfDirty(LastOpenedProject);

                AssetDatabase.Refresh();

                Dirty = false;
            }
        }

        protected virtual void CustomSave(string patchPath, GraphProjectPatch patch)
        {

        }

        public void SaveAs()
        {
            string path = EditorUtility.OpenFolderPanel("Project Folder", "Assets", "");

            if (path == "") return;
            path = LiteraEditorUtilities.ConvertPathToUnityLocal(path);

            string name = "New Project";

            AssetDatabase.Refresh();

            CustomDialogueWindow.Get("Project Name", new(300, 50), s =>
            {
                name = EditorGUILayout.TextField("Name", name);

                if (GUILayout.Button("Create"))
                {
                    GraphWindowInfoAttribute attribute = GetType().GetCustomAttribute<GraphWindowInfoAttribute>();

                    LastOpenedProject = (GraphProject)CreateInstance(attribute == null ? typeof(EndBasedExecutionProject) : attribute.ProjectFileType);
                    LastOpenedProject.ProjectName = name;
                    AssetDatabase.CreateFolder(path, name);
                    AssetDatabase.CreateFolder($"{path}/{name}", "Patches");
                    AssetDatabase.CreateAsset(LastOpenedProject, $"{path}/{name}/Project {name}.asset");
                    AssetDatabase.Refresh();

                    Save();

                    s.Close();
                }
            });
        }

        public void Load(GraphProject project)
        {
            if (project == null || project.Patches.Count == 0) return;

            Loading = true;

            Type[] elementsType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => t.IsSubclassOf(typeof(GraphElement)) && t.GetCustomAttribute<ElementSerializationAttribute>() != null && !t.IsAbstract).ToArray();
            Dictionary<Type, Type> elementsDictionary = new();
            foreach (Type t in elementsType) elementsDictionary.Add(t.GetCustomAttribute<ElementSerializationAttribute>().RuntimeTargetType, t);

            List<GraphElement> loadedGraphElements = new();
            List<RuntimeGraphElement> loadedRuntimeElements = new();

            GraphProjectPatch patch = project.Patches.Last();

            List<RuntimeGraphElement> sortedRuntimeElements = new(patch.Elements);
            sortedRuntimeElements.Sort((a, b) => a.SerializationOrder - b.SerializationOrder);

            foreach (RuntimeGraphElement runtime in sortedRuntimeElements)
            {
                if (elementsDictionary.TryGetValue(runtime.GetType(), out Type elementType))
                {
                    GraphElement element = (GraphElement)Activator.CreateInstance(elementType, new object[] { this });
                    AddElement(element);
                    element.Load(runtime, loadedRuntimeElements, loadedGraphElements, this);
                    loadedRuntimeElements.Add(runtime);
                    loadedGraphElements.Add(element);
                }
                else throw new Exception($"Runtime Graph Element with type \"{runtime.GetType()}\" can not be loaded.");
            }

            CustomLoad(patch);

            LastOpenedProject = project;
            Dirty = false;
            Loading = false;
        }

        protected virtual void CustomLoad(GraphProjectPatch patchConfig)
        {

        }
    }
}