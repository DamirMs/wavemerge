using System;
using Gameplay.Current.Shockwave2048.Enums;
using UnityEngine;

namespace Gameplay.Current.Shockwave2048.Elements
{
    [Serializable]
    public class ElementData
    {
        [SerializeField] private ElementType elementType;
        [SerializeField] private Sprite sprite;
        [SerializeField] private Sprite showcaseSprite;
        
        public ElementType ElementType => elementType;
        public Sprite Sprite => sprite;
        public Sprite ShowcaseSprite => showcaseSprite;
    }
}