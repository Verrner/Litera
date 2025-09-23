using Next.Litera.BuiltIn.OtherGraphElements;
using Next.Litera.Scripting;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Next.Litera.Utilities
{
    public static class LiteraEditorUtilities
    {
        public static string GetHotKeyDescription(string hotkey)
        {
            return string.Join(" + ", hotkey.Select(c =>
            {
                switch (c)
                {
                    case '%': return Application.platform == RuntimePlatform.WindowsEditor ? "Ctrl" : "Cmd";
                    case '^': return "Ctrl";
                    case '#': return "Shift";
                    case '&': return "Alt";
                    default: return c.ToString().ToUpper();
                }
            }));
        }

        public static string ConvertPathToUnityLocal(string path)
        {
            return path.Remove(0, Application.dataPath.Length - 6);
        }

        public static void DisconnectFrom(this IConnectionPort output, IConnectionPort input)
        {
            List<Connection> outputConnections = output.GetOutputConnections();
            int index = outputConnections.FindIndex(o => o.Input == input);
            if (index == -1) return;
            Connection connection = outputConnections[index];

            output.RemoveFromOutputConnections(connection);
            input.RemoveFromInputConnections(connection);

            connection.WindowInstance.RemoveElement(connection);
        }

        public static void DisconnectAll(this IConnectionPort port)
        {
            while (port.GetInputConnections().Count > 0) port.GetInputConnections()[0].Output.DisconnectFrom(port);
            while (port.GetOutputConnections().Count > 0) port.DisconnectFrom(port.GetOutputConnections()[0].Input);
        }

        public static bool ConnectedWith(this IConnectionPort self, IConnectionPort port) => self.GetInputConnections().Exists(c => c.Output == port) || self.GetOutputConnections().Exists(c => c.Input == port);

        public static void ConnectTo(this IConnectionPort output, IConnectionPort input)
        {
            if (output.GetOutputConnections().Exists(c => c.Input == input))
            {
                Debug.LogWarning($"You can connect ports with only one connection. Output port: {output}, Input port: {input}.");
                return;
            }

            GraphElement outputElement = output as GraphElement;
            GraphElement inputElement = input as GraphElement;

            if (outputElement.WindowInstance != inputElement.WindowInstance) throw new System.Exception($"You can not connect elements that are not in the same window.");

            if (!outputElement.WindowInstance.Elements.Contains(outputElement) || !outputElement.WindowInstance.Elements.Contains(inputElement))
            {
                Debug.LogWarning("You can not connect port that is not added to window.");
                return;
            }

            Connection connection = new(input, output);

            output.AddToOutputConnections(connection);
            input.AddToInputConnections(connection);

            outputElement.WindowInstance.InsertElement(Mathf.Min(outputElement.WindowInstance.Elements.IndexOf(outputElement), outputElement.WindowInstance.Elements.IndexOf(inputElement)), connection);
        }
    }
}