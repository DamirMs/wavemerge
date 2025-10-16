using UnityEngine;

namespace Gameplay.Current.Shockwave2048.Elements
{
    public class Element : MonoBehaviour, IMergeable
    {
        [SerializeField] private ElementView view;

        public void Init(ElementData elementData)
        {
            view.Set(elementData.Sprite, (int)elementData.ElementType);
        }
        
        public void PlayMergeEffect()
        {
            view.PlayMerge();
        }

        public void StopMergeEffect()
        {
            view.StopPlayingMerge();
        }
    }
}