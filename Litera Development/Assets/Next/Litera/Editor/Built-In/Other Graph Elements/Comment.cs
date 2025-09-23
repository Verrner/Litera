using Next.Litera.BuiltIn.RuntimeElements;
using Next.Litera.Scripting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Next.Litera.BuiltIn.OtherGraphElements
{
    [PalletteInfo("Tools", "Comment", "", ""), ElementSerialization(typeof(RuntimeComment))]
    public sealed class Comment : GraphElement
    {
        private Label m_commentLabel;

        private string m_commentText = "Enter your comment here";

        [EditorDataSource("comment")]
        public string CommentText
        {
            get => m_commentText;
            set
            {
                m_commentText = value;
                m_commentLabel.text = value;
                Dirty = true;
            }
        }

        private int m_fontSize = 16;

        [EditorDataSource("font-size")]
        public int FontSize
        {
            get => m_fontSize;
            set
            {
                m_fontSize = value;
                m_commentLabel.style.fontSize = m_fontSize;
                Dirty = true;
            }
        }

        private Vector2 m_size = new(100, 100);

        [EditorDataSource("size")]
        public Vector2 Size
        {
            get => m_size;
            set
            {
                m_size = value;
                m_commentLabel.style.width = m_size.x;
                m_commentLabel.style.height = m_size.y;
                Dirty = true;
            }
        }

        private Color m_backgroundColor = Color.clear;

        [EditorDataSource("background-color")]
        public Color BackgroundColor
        {
            get => m_backgroundColor;
            set
            {
                m_backgroundColor = value;
                m_commentLabel.style.backgroundColor = m_backgroundColor;
                Dirty = true;
            }
        }

        private Color m_textColor = new(0.7058f, 0.7058f, 0.7058f);

        [EditorDataSource("text-color")]
        public Color TextColor
        {
            get => m_textColor;
            set
            {
                m_textColor = value;
                m_commentLabel.style.color = m_textColor;
                Dirty = true;
            }
        }

        private TextAnchor m_textAnchor = TextAnchor.MiddleCenter;

        [EditorDataSource("text-anchor")]
        public TextAnchor TextAnchor
        {
            get => m_textAnchor;
            set
            {
                m_textAnchor = value;
                m_commentLabel.style.unityTextAlign = m_textAnchor;
                Dirty = true;
            }
        }

        public Comment(GraphWindow window) : base(window)
        {
            Title = "Comment";

            m_commentLabel = new();
            m_commentLabel.text = CommentText;
            m_commentLabel.style.fontSize = FontSize;
            m_commentLabel.style.whiteSpace = WhiteSpace.Normal;
            m_commentLabel.style.overflow = Overflow.Hidden;
            m_commentLabel.style.textOverflow = TextOverflow.Ellipsis;
            m_commentLabel.style.width = Size.x;
            m_commentLabel.style.height = Size.y;
            m_commentLabel.style.backgroundColor = BackgroundColor;
            m_commentLabel.style.color = TextColor;
            m_commentLabel.style.unityTextAlign = TextAnchor;
            ElementContent.Add(m_commentLabel);
        }

        public override void DrawInspector(Foldout elementFoldout)
        {
            DrawHeader(elementFoldout);
            DrawPositionField(elementFoldout);

            Foldout commentSettings = new() { text = "Comment Settings" };
            elementFoldout.Add(commentSettings);

            Vector2Field sizeField = new("Size");
            sizeField.value = Size;
            sizeField.RegisterValueChangedCallback(c => Size = c.newValue);
            commentSettings.Add(sizeField);

            TextField commentTextField = new("Comment");
            commentTextField.value = m_commentText;
            commentTextField.RegisterValueChangedCallback(c => CommentText = c.newValue);
            commentSettings.Add(commentTextField);

            IntegerField fontSizeField = new("Font Size");
            fontSizeField.value = FontSize;
            fontSizeField.RegisterValueChangedCallback(c => FontSize = c.newValue);
            commentSettings.Add(fontSizeField);

            ColorField backgroundColorField = new("Background Color");
            backgroundColorField.value = BackgroundColor;
            backgroundColorField.RegisterValueChangedCallback(c => BackgroundColor = c.newValue);
            commentSettings.Add(backgroundColorField);

            ColorField textColorField = new("Text Color");
            textColorField.value = TextColor;
            textColorField.RegisterValueChangedCallback(c => TextColor = c.newValue);
            commentSettings.Add(textColorField);

            EnumField textAnchorField = new("Text Anchor", TextAnchor);
            textAnchorField.RegisterValueChangedCallback(c => TextAnchor = (TextAnchor)c.newValue);
            commentSettings.Add(textAnchorField);
        }
    }
}