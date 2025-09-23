using Next.Litera.BuiltIn.OtherGraphElements;
using UnityEngine.UIElements;

namespace Next.Litera.Scripting
{
    public abstract class ConnectionType
    {
        public Connection ParentConnection { get; set; }

        public abstract void DrawInspector(Foldout parentFoldout);
    }
}