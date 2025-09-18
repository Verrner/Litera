using Next.Litera.Scripting;
using UnityEngine;

namespace Next.Litera.BuiltIn.RuntimeElements
{
    public sealed class RuntimeComment : RuntimeGraphElement
    {
        [RuntimeDataSource("comment")] public string Comment;
        [RuntimeDataSource("font-size")] public int FontSize;
        [RuntimeDataSource("size")] public Vector2 Size;
        [RuntimeDataSource("background-color")] public Color BackgroundColor;
        [RuntimeDataSource("text-color")] public Color TextColor;
        [RuntimeDataSource("text-anchor")] public TextAnchor TextAnchor;
    }
}