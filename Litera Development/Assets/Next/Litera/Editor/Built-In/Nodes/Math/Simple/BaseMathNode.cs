using UnityEngine.UIElements;

namespace Next.Litera.Scripting
{
    public abstract class BaseMathNode : LiteraNode
    {
        [EditorDataSource("a"), DataTransportInput("a")] public float a;
        [EditorDataSource("b"), DataTransportInput("b")] public float b;
        [DataTransportOutput("result")] public float result;

        protected BaseMathNode(GraphWindow window) : base(window)
        {
        }

        public override void DrawInspector(Foldout elementFoldout)
        {
            base.DrawInspector(elementFoldout);

            FloatField aField = new("A");
            aField.value = a;
            aField.RegisterValueChangedCallback(c =>
            {
                a = c.newValue;
                Dirty = true;
            });
            elementFoldout.Add(aField);

            FloatField bField = new("B");
            bField.value = b;
            bField.RegisterValueChangedCallback(c =>
            {
                b = c.newValue;
                Dirty = true;
            });
            elementFoldout.Add(bField);
        }
    }
}