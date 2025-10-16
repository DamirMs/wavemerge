using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Current.Shockwave2048.Elements
{
    [Serializable]
    public class ElementView
    {
        [SerializeField] private Image image;
        [SerializeField] private Image mergeImage;
        [SerializeField] private TextMeshProUGUI levelText;

        public void Set(Sprite sprite, int level)
        {
            image.sprite = sprite;
            levelText.text = level.ToString();
            
            mergeImage.enabled = false;
        } 
        
        public void PlayMerge()
        {
            mergeImage.enabled = true;
        }

        public void StopPlayingMerge()
        {
            mergeImage.enabled = false;
        }
    }
}