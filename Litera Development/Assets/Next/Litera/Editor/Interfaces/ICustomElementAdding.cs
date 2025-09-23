using Next.Litera.Scripting;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Next.Litera
{
    public interface ICustomElementAdding
    {
        void CustomAdding(GraphWindow window, List<GraphElement> elements, VisualElement resizableElement);
    }
}