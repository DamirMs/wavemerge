using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Current.Configs;
using Gameplay.Current.Shockwave2048.Board;
using Gameplay.Current.Shockwave2048.Elements;
using Gameplay.Current.Shockwave2048.Enums;
using PT.Tools.EventListener;
using UnityEngine;
using Zenject;
using IPromise = Assets.Scripts.Libraries.RSG.IPromise;

namespace Gameplay.Current.Shockwave2048
{
    public class ShockwaveProcessor : MonoBehaviourEventListener
    {
        [Inject] private GameInfoConfig _gameInfoConfig;
        [Inject] private BoardManager _board;

        private Queue<Element> _pendingShockwaves = new();
        
        private CancellationTokenSource _cts;
        
        private void Awake()
        {
            
            AddEventActions(new()
            {
                { GlobalEventEnum.GameEnded, OnGameFinished},
            });
        }
        
        public async UniTask ProcessTurn(Vector2Int origin)
        {
            _cts?.Cancel();
            _cts = new();

            try
            {
                await PerformShockwave(origin, _cts.Token);
        
                while (_pendingShockwaves.Count > 0)
                {
                    var shockwaveElement = _pendingShockwaves.Dequeue();
                    _board.Merge(shockwaveElement);
                    
                    if (!_board.IsInBoard(shockwaveElement)) continue;
            
                    await PerformShockwave(_board.GetPosition(shockwaveElement), _cts.Token);
                }

                GlobalEventBus.On(GlobalEventEnum.PlayerTurn);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Shockwave processing canceled.");
            }
        }
        
        private void OnGameFinished()
        {
            _cts?.Cancel();
        }

        private async UniTask PerformShockwave(Vector2Int origin, CancellationToken token)
        {
            var dirs = (DirectionEnum[])Enum.GetValues(typeof(DirectionEnum));
            var tasks = new List<UniTask>();

            foreach (var dir in dirs) tasks.Add(PushAndMerge(origin, dir, token));
            
            await UniTask.WhenAll(tasks);
        }
        
        private async UniTask PushAndMerge(Vector2Int origin, DirectionEnum dir, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            
            var allMovements = new List<UniTask>();
            var alreadyMerged = new HashSet<Vector2Int>();
    
            foreach (var pos in GetLinePositions(origin, dir))
            {
                token.ThrowIfCancellationRequested();
                
                var elementType = _board.Get(pos);
                if (elementType == ElementType.Empty) continue;

                var target = FindFarthestValidPosition(pos, Utils.GetDirection(dir), alreadyMerged);
                if (target != pos)
                {
                    IPromise movePromise;
                    
                    if (_board.Get(pos) == _board.Get(target))
                    {
                        alreadyMerged.Add(target);

                        movePromise = _board.MergeMove(pos, target, out Element mergedElement);
                        _pendingShockwaves.Enqueue(mergedElement);
                    }
                    else
                    {
                        movePromise = _board.Move(pos, target);
                    }
                    
                    allMovements.Add(movePromise.ToUniTask());
                }
            }

            await UniTask.WhenAll(allMovements);
        }

        private IEnumerable<Vector2Int> GetLinePositions(Vector2Int origin, DirectionEnum dir)
        {
            switch (dir)
            {
                case DirectionEnum.Right:
                    for (int x = _gameInfoConfig.GridWidth - 1; x > origin.x; x--)
                        yield return new Vector2Int(x, origin.y);
                    break;
                case DirectionEnum.Left:
                    for (int x = 0; x < origin.x; x++)
                        yield return new Vector2Int(x, origin.y);
                    break;
                case DirectionEnum.Up:
                    for (int y = _gameInfoConfig.GridWidth - 1; y > origin.y; y--)
                        yield return new Vector2Int(origin.x, y);
                    break;
                case DirectionEnum.Down:
                    for (int y = 0; y < origin.y; y++)
                        yield return new Vector2Int(origin.x, y);
                    break;
            }
        }

        private Vector2Int FindFarthestValidPosition(Vector2Int from, Vector2Int dir, HashSet<Vector2Int> alreadyMerged)
        {
            var farthest = from;
            var next = farthest + dir;
            
            var fromType = _board.Get(from);
            
            while (_board.IsInside(next))
            {
                var targetType = _board.Get(next);
                
                if (targetType == ElementType.Empty)
                {
                    farthest = next;
                }
                else if (targetType == fromType && !alreadyMerged.Contains(next))
                {
                    farthest = next;
                    
                    break;
                }
                else break;
                
                next += dir;
            }
            
            return farthest;
        }
    }
}