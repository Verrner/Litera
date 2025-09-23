using Next.Litera.Scripting;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace Next.Litera.BuiltIn.ConnectionTypes
{
    public abstract class DataTransportConnectionType : ConnectionType
    {
        [EditorDataSource("output-member-key")] public string OutputKey;
        [EditorDataSource("input-member-key")] public string InputKey;

        public override void DrawInspector(Foldout parentFoldout)
        {
            parentFoldout.Clear();

            PrepareOutputDropdown(parentFoldout);
            PrepareInputDropdown(parentFoldout);
            DrawConnectionType(parentFoldout);
        }

        protected abstract void DrawConnectionType(Foldout parentFoldout);

        private void PrepareOutputDropdown(Foldout foldout)
        {
            List<string> keys = new();
            List<string> paths = new();

            foreach (MemberInfo member in ParentConnection.Output.GetType().GetMembers())
            {
                PropertyInfo prop = member as PropertyInfo;
                FieldInfo field = member as FieldInfo;

                if (field == null && prop == null) continue;

                DataTransportOutputAttribute a = member.GetCustomAttribute<DataTransportOutputAttribute>();
                if (a == null) continue;

                if (prop != null && !prop.CanRead) throw new System.Exception($"Property {prop.Name} of element of type {ParentConnection.Output.GetType()} must contain getter.");

                paths.Add($"{(prop != null ? prop.PropertyType : field.FieldType)}/{member.Name}");
                if (keys.Contains(a.Key)) throw new System.Exception($"Element of type {ParentConnection.Output.GetType()} has at least two data transport outputs with same key \"{a.Key}\"");
                keys.Add(a.Key);
            }

            if (paths.Count == 0)
            {
                foldout.Add(new HelpBox("Output connection has not any outputs.", HelpBoxMessageType.Info));
                return;
            }

            int defaultIndex = keys.IndexOf(OutputKey);
            if (defaultIndex == -1)
            {
                OutputKey = keys[0];
                defaultIndex = 0;
            }

            List<string> choices = new();
            for (int i = 0; i < paths.Count; i++) choices.Add($"{paths[i]} ({keys[i]})");
            choices.Sort();

            DropdownField dropdown = new("Output", choices, defaultIndex);
            dropdown.RegisterValueChangedCallback(e =>
            {
                OutputKey = keys[choices.IndexOf(e.newValue)];
            });
            foldout.Add(dropdown);
        }

        private void PrepareInputDropdown(Foldout foldout)
        {
            List<string> paths = new();
            List<string> keys = new();

            foreach (MemberInfo member in ParentConnection.Input.GetType().GetMembers())
            {
                PropertyInfo prop = member as PropertyInfo;
                FieldInfo field = member as FieldInfo;

                if (prop == null && field == null) continue;

                DataTransportInputAttribute a = member.GetCustomAttribute<DataTransportInputAttribute>();
                if (a == null) continue;

                if (prop != null && !prop.CanWrite) throw new System.Exception($"Property {prop.Name} of element of type {ParentConnection.Output.GetType()} must contain setter.");

                paths.Add($"{(prop != null ? prop.PropertyType : field.FieldType)}/{member.Name}");
                if (keys.Contains(a.Key)) throw new System.Exception($"Element of type {ParentConnection.Output.GetType()} has at least two data transport inputs with same key \"{a.Key}\"");
                keys.Add(a.Key);
            }

            if (paths.Count == 0)
            {
                foldout.Add(new HelpBox("Input connection has not any inputs.", HelpBoxMessageType.Info));
                return;
            }

            int defaultIndex = keys.IndexOf(InputKey);
            if (defaultIndex == -1)
            {
                InputKey = keys[0];
                defaultIndex = 0;
            }

            List<string> choices = new();
            for (int i = 0; i < paths.Count; i++) choices.Add($"{paths[i]} ({keys[i]})");
            choices.Sort();

            DropdownField dropdown = new("Input", choices, defaultIndex);
            dropdown.RegisterValueChangedCallback(e =>
            {
                InputKey = keys[choices.IndexOf(e.newValue)];
            });
            foldout.Add(dropdown);
        }
    }
}