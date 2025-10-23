using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Current.Configs;
using Gameplay.Current.Shockwave2048.Elements;
using Gameplay.Current.Shockwave2048.Enums;
using Gameplay.General.Score;
using Gameplay.IOS.Shop;
using PT.Tools.EventListener;
using UnityEngine;
using Zenject;
using IPromise = Assets.Scripts.Libraries.RSG.IPromise;

namespace Gameplay.Current.Shockwave2048.Board
{
    public class BoardManager : MonoBehaviourEventListener
    {
        [SerializeField] private BoardView boardView;

        public event Action<Vector2> OnMergePush;
        public event Action OnItemPlaced;
        
        [Inject] private GameInfoConfig _gameInfoConfig;
        [Inject] private GameplayController _gameplayController;
        [Inject] private ShockwaveProcessor _shockwaveProcessor;
        [Inject] private ElementProvider _elementProvider;
        [Inject] private ScoreManager _scoreManager;
        [Inject] private ShopManager _shopManager;
        [Inject] private ShopItemsConfig _shopItemsConfig;
        
        private Dictionary<Vector2Int, GridSlotData> _grid = new();
        private Stack<Dictionary<Vector2Int, GridSlotData>> _gridSteps = new();
        
        private ElementType _nextElementType;

        private int _mergeStep;
        
        private void Awake()
        {
            AddEventActions(new()
            {
                { GlobalEventEnum.GameStarted, OnGameStarted},
                { GlobalEventEnum.GameEnded, OnGameFinished},
                { GlobalEventEnum.PlayerTurn, OnPlayerTurn},
                { GlobalEventEnum.UndoTurn, OnUndoTurn},
            });
            
            boardView.Init(this, _gameInfoConfig.GridWidth, ref _grid);
        }

        private void OnGameStarted()
        {
            _mergeStep = 1;
        }
        private void OnGameFinished()
        {
            ClearGrid();
            _gridSteps.Clear();
        }

        private void OnPlayerTurn()
        {
            _mergeStep = 1;
        }

        private void OnUndoTurn()
        {
            if (_gridSteps.Count == 0) return;
            
            ClearGrid();

            var lastTurnGrid = _gridSteps.Pop();

            foreach (var kvp in lastTurnGrid)
            {
                if (kvp.Value.ElementType != ElementType.Empty)
                {
                    InstantiateElementAt(kvp.Key, _elementProvider.GetData(kvp.Value.ElementType), kvp.Value.WorldPosition);
                }
            }
        }
        
        public void SetNextElement(ElementType nextElementType) => _nextElementType = nextElementType;
        
        public bool IsInside(Vector2Int pos) =>
            pos.x >= 0 && pos.x < _gameInfoConfig.GridWidth && pos.y >= 0 && pos.y < _gameInfoConfig.GridWidth;
        public ElementType Get(Vector2Int pos) => _grid[pos].ElementType;
        public bool IsInBoard(Element element) 
        {
            var kvp = GetKvpByElement(element);
            return kvp.Value != null;
        }
        public Vector2Int GetPosition(Element element) => GetKvpByElement(element).Key;

        public IPromise MergeMove(Vector2Int pos, Vector2Int target, out Element mergedElement)
        {
            mergedElement = _grid[pos].Element;
            var destroyElement = _grid[target].Element;

            var promise = boardView.MergeMove(
                    mergedElement, _grid[target].WorldPosition, 
                    _gameInfoConfig.ElementSpeedComboProgression[0], 
                    destroyElement, _gameInfoConfig.ElementWidth)
                .Then(() =>
                {
                    _grid[target].SetData(_elementProvider.GetNext(_grid[target].ElementType));
                    boardView.ReturnElement(destroyElement);
                });
            
            SwapSlots(pos, target);

            return promise;
        }
        public IPromise Move(Vector2Int pos, Vector2Int target)
        {
            var promise = boardView.Move(_grid[pos].Element, _grid[target].WorldPosition, _gameInfoConfig.ElementSpeedComboProgression[0]);

            SwapSlots(pos, target);

            return promise;
        }

        public void Merge(Element mergedElement)
        {
            var gridKvp = GetKvpByElement(mergedElement);
            
            OnMergePush?.Invoke(gridKvp.Value.WorldPosition);
            _scoreManager.MergePush(gridKvp.Value.WorldPosition, (int)gridKvp.Value.ElementType, _mergeStep);
            
            _mergeStep++;
            
            mergedElement.StopMergeEffect();

            CheckLimitTypeReached(gridKvp.Value);
        }

        public void TryPlaceElement(Vector2Int slotPosition)
        {
            if (!_gameplayController.IsPlayerTurn || 
                (_grid[slotPosition].ElementType != ElementType.Empty && _grid[slotPosition].ElementType != _nextElementType)) return;
            _gridSteps.Push(DeepCopyGrid(_grid));
            
            OnItemPlaced?.Invoke();
            
            if (_grid[slotPosition].ElementType == ElementType.Empty)
            {
                InstantiateElementAt(slotPosition, _elementProvider.GetData(_nextElementType), _grid[slotPosition].WorldPosition);
            }
            else if (_grid[slotPosition].ElementType == _nextElementType)
            {
                _grid[slotPosition].SetData(_elementProvider.GetNext(_grid[slotPosition].ElementType));
                
                _scoreManager.MergePush(_grid[slotPosition].WorldPosition, (int)_grid[slotPosition].ElementType, _mergeStep);
                
                CheckLimitTypeReached(_grid[slotPosition]);
            }
            
            _shockwaveProcessor.ProcessTurn(slotPosition);
            
            boardView.PlayPush(_grid[slotPosition].WorldPosition);
        }
        
        public bool NoPlayableSteps() => 
            _grid.All(kvp => kvp.Value.ElementType != ElementType.Empty && kvp.Value.ElementType != _nextElementType);

        private void SwapSlots(Vector2Int pos, Vector2Int target)
        {
            _grid[target].SetData(_elementProvider.GetData(_grid[pos].ElementType), _grid[pos].Element);
            _grid[pos].Clear();
        }

        private void InstantiateElementAt(Vector2Int gridPosition, ElementData data, Vector2 position)
        {
            var element = boardView.GetElement();
            element.transform.localPosition = position;
            
            var index = _shopManager.GetEquippedId(ShopItemEnum.ShopMainItem);
            element.SetShopSprite(_shopItemsConfig.ShopItemInfos[index].Sprite);
            
            _grid[gridPosition].SetData(data, element);
        }

        private void CheckLimitTypeReached(GridSlotData data)
        {
            if (data.ElementType == ElementType.TwoFiftySix)
            {
                boardView.ReturnElement(data.Element);
                
                data.Clear();
            }
        }
        
        private void ClearGrid()
        {
            foreach (var kvp in _grid)
            {
                if (kvp.Value.ElementType != ElementType.Empty) 
                    boardView.ReturnElement(kvp.Value.Element);
                
                kvp.Value.Clear();
            }
        }
        
        private KeyValuePair<Vector2Int, GridSlotData> GetKvpByElement(Element element) 
            => _grid.FirstOrDefault(kvp => kvp.Value.Element == element);

        private Dictionary<Vector2Int, GridSlotData> DeepCopyGrid(Dictionary<Vector2Int, GridSlotData> original)
        {
            var copy = new Dictionary<Vector2Int, GridSlotData>();
            
            foreach (var kvp in original)
            {
                var copiedSlot = new GridSlotData(kvp.Value.WorldPosition)
                {
                    ElementType = kvp.Value.ElementType
                };
                copy.Add(kvp.Key, copiedSlot);
            }
            
            return copy;
        }
    }
}