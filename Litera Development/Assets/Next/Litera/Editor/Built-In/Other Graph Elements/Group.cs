using UnityEngine.UIElements;

namespace Next.Litera.BuiltIn.OtherGraphElements
{
    [PalletteInfo("Tools", "Group", "")]
    public class Group : ScopeElement
    {
        private Label m_titleLabel;

        public Group(GraphWindow window) : base(window)
        {
            VisualElement titleLabelContainer = new();
            titleLabelContainer.AddToClassList("title-label-container");
            ScopeContainer.Insert(0, titleLabelContainer);

            Title = "New Group";

            m_titleLabel = new(Title);
            m_titleLabel.AddToClassList("title-label");
            m_titleLabel.style.maxWidth = 80;
            m_titleLabel.style.color = Color;
            titleLabelContainer.Add(m_titleLabel);

            TitleChanged += () => m_titleLabel.text = Title;
            ColorChanged += () => m_titleLabel.style.color = Color;
            ScopeSizeRefreshed += () => m_titleLabel.style.maxWidth = ScopeChildrenContentContainer.style.width.value.value - 20;
        }

        public override void DrawInspector(Foldout elementFoldout)
        {
            DrawTitleField(elementFoldout);
            base.DrawInspector(elementFoldout);
        }
    }
}