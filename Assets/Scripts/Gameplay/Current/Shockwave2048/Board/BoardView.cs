using System;
using System.Collections.Generic;
using Assets.Scripts.Libraries.RSG;
using DG.Tweening;
using Gameplay.Current.Shockwave2048.Elements;
using PT.Tools.ObjectPool;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Current.Shockwave2048.Board
{
    [Serializable]
    public class BoardView
    {
        [SerializeField] private Button[] slotsButtons;
        [Space]
        [SerializeField] private Element elementPrefab;
        [SerializeField] private Transform elementsParent;
        [Space]
        [SerializeField] private GameObject pushEffect;
        [SerializeField] private Transform pushEffectsParent;
        [SerializeField] private float pushEffectTargetScale = 5;
        [SerializeField] private float pushEffectDuration = 0.75f;
        
        private Vector2 _pushEffectInitialScale;

        private ObjectPool _pushEffectPool = new();
        private readonly ElementPool _elementPool = new();
        
        public void Init(BoardManager boardManager, int gridWidth, ref Dictionary<Vector2Int, GridSlotData> grid)
        {
            for (int i = 0; i < slotsButtons.Length; i++)
            {
                var pos = new Vector2Int(i % gridWidth, i / gridWidth);
                slotsButtons[i].onClick.AddListener(() => boardManager.TryPlaceElement(pos));
                
                grid.Add(pos, new(slotsButtons[i].transform.localPosition));
            }
            
            _pushEffectInitialScale =  pushEffectsParent.localScale;
            _pushEffectPool.Init(pushEffect, pushEffectsParent, gridWidth);

            boardManager.OnMergePush += PlayPush; //SIGN OUT TOO
            
            _elementPool.Init(elementPrefab.gameObject, elementsParent, slotsButtons.Length);
        }
        
        public IPromise MergeMove(Element elementMoving, Vector2 destination, float duration, Element elementTo, float mergeDistance)
        {
            return elementMoving.transform
                .DOLocalMove(destination, duration)
                .SetEase(Ease.OutQuart)
                .OnUpdate(() =>
                {
                    if (Vector2.Distance(elementMoving.transform.position, elementTo.transform.position) < mergeDistance)
                    {
                        elementMoving.PlayMergeEffect();
                        elementTo.PlayMergeEffect();
                    }
                }).ToPromise();
        }
        
        public IPromise Move(Element elementMoving, Vector2 destination, float duration)
        {
            return elementMoving.transform
                .DOLocalMove(destination, duration)
                .SetEase(Ease.OutQuart)
                .ToPromise();
        }

        public void PlayPush(Vector2 position)//myb move to other script
        {
            var effectObj = _pushEffectPool.Get();

            effectObj.transform.localPosition = position;

            effectObj.transform.DOScale(pushEffectTargetScale, pushEffectDuration )
                .OnComplete(() =>
                {
                    _pushEffectPool.Set(effectObj);
                    effectObj.transform.localScale = _pushEffectInitialScale;
                });
        }

        public Element GetElement() => _elementPool.Get();
        public void ReturnElement(Element element) => _elementPool.Set(element);
    }
    
    class ElementPool : PoolBase<Element>
    {
        protected override Element CreateObject()
        {
            return GameObject.Instantiate(_prefab, _parent).GetComponent<Element>();
        }
        protected override void OnGet(Element obj)
        {
            obj.SetActive(true);
        }
        protected override void OnSet(Element obj)
        {
            obj.SetActive(false);
        }
    }
}