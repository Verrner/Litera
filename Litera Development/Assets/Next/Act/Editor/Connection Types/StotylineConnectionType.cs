using Next.Litera;
using Next.Litera.Scripting;
using UnityEngine.UIElements;

namespace Next.Act
{
    [ConnectionType("Storyline", typeof(StorylineRuntimeConnectionType))]
    public sealed class StorylineConnectionType : ConnectionType
    {
        public override void DrawInspector(Foldout parentFoldout)
        {

        }
    }
}