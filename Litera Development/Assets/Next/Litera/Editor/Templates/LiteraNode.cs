using Next.Litera.BuiltIn.OtherGraphElements;
using Next.Litera.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Next.Litera.Scripting
{
    public abstract class LiteraNode : GraphElement, IConnectionPort
    {
        protected VisualElement mainContainer { get; private set; }
        protected VisualElement titleContainer { get; private set; }

        private List<Connection> m_inputConnections = new();
        private List<Connection> m_outputConnections = new();

        public List<Connection> InputConnections => new(m_inputConnections);
        public List<Connection> OutputConnections => new(m_outputConnections);

        private VisualElement m_nodeContentContainer;
        protected VisualElement nodeContentContainer
        {
            get
            {
                if (m_nodeContentContainer == null)
                {
                    m_nodeContentContainer = new();
                    m_nodeContentContainer.name = "node-content-container";
                    m_nodeContentContainer.AddToClassList("node-content-container");
                    mainContainer.Add(m_nodeContentContainer);
                }
                return m_nodeContentContainer;
            }
        }

        private Label m_titleLabel;

        public override string Title
        {
            get { return base.Title; }
            set
            {
                m_titleLabel.text = value;
                base.Title = value;
            }
        }

        public LiteraNode(GraphWindow window) : base(window)
        {
            ElementContent.styleSheets.Add(Resources.Load<StyleSheet>("Styles/Graph Node Styles"));

            mainContainer = new();
            mainContainer.name = "main-container";
            mainContainer.AddToClassList("main-container");
            ElementContent.Add(mainContainer);

            titleContainer = new();
            titleContainer.name = "title-container";
            titleContainer.AddToClassList("title-container");
            mainContainer.Add(titleContainer);

            m_titleLabel = new();
            m_titleLabel.name = "title-label";
            m_titleLabel.AddToClassList("title-label");
            titleContainer.Add(m_titleLabel);

            Title = GetType().ToString();

            Dirty = false;

            BeforeDeletion += this.DisconnectAll;
        }

        public Vector2 GetInputPortPosition() => Position + new Vector2(5, resolvedStyle.height / 2 - 10);

        public Vector2 GetOutputPortPosition() => Position + new Vector2(resolvedStyle.width - 25, resolvedStyle.height / 2 - 10);

        public void RegisterCallbacks(Connection connection)
        {
            PositionChanged += connection.RefreshEdges;
            SizeChanged += connection.RefreshEdges;
        }

        public void UnregisterCallbacks(Connection connection)
        {
            PositionChanged -= connection.RefreshEdges;
            SizeChanged -= connection.RefreshEdges;
        }

        public void RemoveFromOutputConnections(Connection connection) => m_outputConnections.Remove(connection);

        public void RemoveFromInputConnections(Connection connection) => m_inputConnections.Remove(connection);

        public void AddToOutputConnections(Connection connection) => m_outputConnections.Add(connection);

        public void AddToInputConnections(Connection connection) => m_inputConnections.Add(connection);

        public List<Connection> GetOutputConnections() => m_outputConnections;

        public List<Connection> GetInputConnections() => m_inputConnections;
    }
}