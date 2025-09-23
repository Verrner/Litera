using Next.Litera.BuiltIn.RuntimeNodes;
using Next.Litera.Scripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Next.Litera.BuiltIn.Nodes
{
    [PalletteInfo("Tools", "Debug Log", "", "", "debug"), ElementSerialization(typeof(DebugLogRuntimeNode))]
    public sealed class DebugLogNode : LiteraNode
    {
        private DebugLogType m_type;

        [EditorDataSource("type")] public DebugLogType Type
        {
            get => m_type;
            set
            {
                m_type = value;
            }
        }

        private string m_message = "Message";

        [EditorDataSource("message"), DataTransportInput("message")] public string Message
        {
            get => m_message;
            set
            {
                m_message = value;
                m_messageLabel.text = m_message;
            }
        }

        private Label m_messageLabel;

        public DebugLogNode(GraphWindow window) : base(window)
        {
            Title = "Debug Log";

            m_messageLabel = new();
            m_messageLabel.text = Message;
            m_messageLabel.AddToClassList("message-label");
            nodeContentContainer.Add(m_messageLabel);

            nodeContentContainer.styleSheets.Add(Resources.Load<StyleSheet>("Styles/Debug Log Node Styles"));
        }

        public override void DrawInspector(Foldout elementFoldout)
        {
            base.DrawInspector(elementFoldout);

            EnumField typeField = new("Type", Type);
            typeField.RegisterValueChangedCallback(e =>
            {
                Type = (DebugLogType)e.newValue;
                Dirty = true;
            });
            elementFoldout.Add(typeField);

            TextField message = new("Message");
            message.value = Message;
            message.RegisterValueChangedCallback(e =>
            {
                Message = e.newValue;
                Dirty = true;
            });
            elementFoldout.Add(message);
        }
    }
}