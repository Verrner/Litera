using UnityEngine;

namespace Next.Litera.Scripting
{
    [System.Serializable]
    public abstract class RuntimeGraphElement : ScriptableObject
    {
        public int SerializationOrder;

        [RuntimeDataSource("position")] public Vector2 Position;
        [RuntimeDataSource("title")] public string Title;
        [RuntimeDataSource("enable-sprite")] public bool EnableSprite;
        [RuntimeDataSource("sprite")] public Sprite Sprite;
        [RuntimeDataSource("sprite-size")] public float SpriteSize;
    }
}