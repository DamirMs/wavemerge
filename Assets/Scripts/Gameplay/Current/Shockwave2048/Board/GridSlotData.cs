using Gameplay.Current.Shockwave2048.Elements;
using Gameplay.Current.Shockwave2048.Enums;
using UnityEngine;

namespace Gameplay.Current.Shockwave2048.Board
{
    public class GridSlotData
    {
        public ElementType ElementType = ElementType.Empty;
        public Vector2 WorldPosition;
        public Element Element;

        public GridSlotData(Vector2 worldPosition)
        {
            WorldPosition = worldPosition;
            ElementType = ElementType.Empty;
        }

        public void SetData(ElementData data, Element element)
        {
            Element = element;
            SetData(data);
        }
        public void SetData(ElementData data)
        {
            ElementType = data.ElementType;
            Element.Init(data);
        }

        public void Clear()
        {
            ElementType = ElementType.Empty;
            Element = null;
        }
    }
}