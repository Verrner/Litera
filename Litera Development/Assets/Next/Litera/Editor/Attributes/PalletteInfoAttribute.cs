using System;
using UnityEngine;

namespace Next.Litera
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PalletteInfoAttribute : Attribute
    {
        public readonly string GroupName;
        public readonly string Name;
        public readonly string Tooltip;
        public readonly Sprite Sprite;
        public readonly bool EnableInAllWindows;
        public readonly string[] ElementsCollections;

        /// <param name="groupName">Name of pallette's group foldout.</param>
        /// <param name="name">Name of pallette element.</param>
        /// <param name="spritePath">Path of element's sprite in Resources folder.</param>
        /// <param name="tooltip">Tooltip of element.</param>
        public PalletteInfoAttribute(string groupName, string name, string spritePath, string tooltip = "")
        {
            GroupName = groupName;
            Name = name;
            Tooltip = tooltip;
            Sprite = Resources.Load<Sprite>(spritePath);
            EnableInAllWindows = true;
        }

        /// <param name="groupName">Name of pallette's group foldout.</param>
        /// <param name="name">Name of pallette element.</param>
        /// <param name="spritePath">Path of element's sprite in Resources folder.</param>
        /// <param name="tooltip">Tooltip of element.</param>
        public PalletteInfoAttribute(string groupName, string name, string spritePath, string tooltip = "", params string[] elementsCollections) : this(groupName, name, spritePath, tooltip)
        {
            EnableInAllWindows = false;
            ElementsCollections = elementsCollections;
        }
    }
}