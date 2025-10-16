using System.Collections.Generic;
using Gameplay.Current.Configs;
using Gameplay.Current.Shockwave2048.Enums;
using UnityEngine;
using Zenject;

namespace Gameplay.Current.Shockwave2048.Elements.NextElement
{
    public class NextElementManager : MonoBehaviour
    {
        [SerializeField] private NextElementView nextElementView;
        
        [Inject] private ElementProvider _elementProvider;
        [Inject] private GameInfoConfig _gameInfoConfig;

        private Stack<ElementType> _previousNextElements = new();
        private ElementType _currentNextElement;

        public ElementType GetNext()
        {
            var type = _gameInfoConfig.PlayableElementTypes.GetRandomElement();
            SetNext(type);
            _currentNextElement = type;
            
            return type;
        }

        public void SaveCurrent()
        {
            _previousNextElements.Push(_currentNextElement);
        }

        public ElementType Undo()
        {
            if (_previousNextElements.Count == 0) return _currentNextElement;

            var prev = _previousNextElements.Pop();
            SetNext(prev);
            
            return prev;
        }

        public void Reset()
        {
            _previousNextElements.Clear();
        }

        private void SetNext(ElementType type)
        {
            var data = _elementProvider.GetData(type);
            nextElementView.Set(data.ShowcaseSprite, (int)type);
        }
    }
}