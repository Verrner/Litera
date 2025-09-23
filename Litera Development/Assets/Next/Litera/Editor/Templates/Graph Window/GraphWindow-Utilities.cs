using Next.Litera.Scripting;
using Next.Litera.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Next.Litera
{
    public abstract partial class GraphWindow
    {
        public void RefreshTitle()
        {
            titleContent.text = $"{m_title} ({(m_lastOpenedProject == null ? "Unnamed" : m_lastOpenedProject.ProjectName)}){(m_dirty ? " *" : "")}";
        }

        public GenericMenu GetGenericMenu()
        {
            GenericMenu menu = new();

            IConnectionPort portA = null;
            IConnectionPort portB = null;
            foreach (GraphElement element in m_selection)
            {
                if (element is IConnectionPort port)
                {
                    if (portA == null) portA = port;
                    else if (portB == null) portB = port;
                    else break;
                }
            }
            if (portA != null && portB != null) menu.AddItem(new("Connect"), false, () => portA.ConnectTo(portB));
            else menu.AddDisabledItem(new("Connect"), false);

            GetCustomContextualMenu(menu);

            return menu;
        }

        protected virtual void GetCustomContextualMenu(GenericMenu menu)
        {

        }

        public Vector2 LocalToGridPosition(Vector2 position)
        {
            Vector2 halfOfWindow = new Vector2(this.position.width * .5f, this.position.height * .5f);
            Vector2 localWindowPos = position - halfOfWindow;
            Vector2 scaledLocalWindowPos = localWindowPos / Zoom;
            return scaledLocalWindowPos - CameraPosition + halfOfWindow;
        }

        public Vector2 GridToLocalPosition(Vector2 position)
        {
            Vector2 halfOfWindow = new Vector2(this.position.width * .5f, this.position.height * .5f);
            Vector2 scaledLocalWindowPos = position + CameraPosition - halfOfWindow;
            scaledLocalWindowPos *= Zoom;
            return scaledLocalWindowPos + halfOfWindow;
        }

        public static GraphWindow GetWindowInstance(string name)
        {
            if (m_instances.TryGetValue(name, out GraphWindow res)) return res;
            throw new System.Exception($"Litera graph window with name \"{name}\" is not found.");
        }

        public static bool TryGetWindowInstance(string name, out GraphWindow window) => m_instances.TryGetValue(name, out window);

        public static T GetWindowInstance<T>(string name) where T : GraphWindow
        {
            if (m_instances.TryGetValue(name, out GraphWindow res))
            {
                if (res is T result) return result;
                throw new System.Exception($"Litera graph window with name \"{name}\" is found, but can not be parsed to {typeof(T)}.");
            }
            throw new System.Exception($"Litera graph window with name \"{name}\" is not found.");
        }

        public static bool WindowInstanceExists(string name) => m_instances.ContainsKey(name);

        public static List<string> GetAllWindowsKeys() => m_instances.Keys.ToList();

        public static List<GraphWindow> GetAllWindows() => m_instances.Values.ToList();

        protected static string RegisterWindowInstanceUnique(GraphWindow window)
        {
            return RegisterWindowInstanceUnique(window, window.GetType().Name);
        }

        protected static string RegisterWindowInstanceUnique(GraphWindow window, string name)
        {
            string resName = name;
            int index = 0;
            if (WindowInstanceExists(name))
            {
                while (WindowInstanceExists($"{name} {index}")) index++;
                resName = $"{name} {index}";
            }

            window.WindowName = resName;
            m_instances.Add(resName, window);
            return resName;
        }
    }
}