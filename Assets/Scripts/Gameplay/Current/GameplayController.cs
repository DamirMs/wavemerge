using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Current.Configs;
using Gameplay.Current.Shockwave2048.Board;
using Gameplay.Current.Shockwave2048.Elements;
using Gameplay.Current.Shockwave2048.Elements.NextElement;
using Gameplay.General.Game;
using Gameplay.General.Windows;
using PT.Tools.EventListener;
using UnityEngine;
using Zenject;

namespace Gameplay.Current
{
    public class GameplayController : LevelGameplayController
    {
        [SerializeField] private NextElementManager nextElementManager;
        
        public bool IsPlayerTurn { get; private set; }
        
        [Inject] private ElementProvider _elementProvider;
        [Inject] private BoardManager _boardManager;

        private int _step;
        
        protected override void Awake()
        {
            base.Awake();

            AddEventActions(new()
            {
                { GlobalEventEnum.PlayerTurn, OnPlayerTurn },
                { GlobalEventEnum.UndoTurn, OnUndoTurn },
            });
        }
        
        protected override void SignUp()
        {
            _boardManager.OnItemPlaced += OnGameTurn;
        }
        
        protected override void SignOut()
        {
            _boardManager.OnItemPlaced -= OnGameTurn;
        }
        
        public void TryUndoTurn()
        {
            if (!IsPlayerTurn || _step <= 0) return;
            
            GlobalEventBus.On(GlobalEventEnum.UndoTurn);
        }

        protected override void OnGameStarted()
        {
            base.OnGameStarted();
            
            _step = 0;
            nextElementManager.Reset();
            GlobalEventBus.On(GlobalEventEnum.PlayerTurn);
        }

        private void OnPlayerTurn()
        {
            IsPlayerTurn = true;

            var nextElement = nextElementManager.GetNext();
            _boardManager.SetNextElement(nextElement);

            if (_boardManager.NoPlayableSteps())
            {
                GameOver().Forget();
            }

            _step++;
        }

        private void OnGameTurn()
        {
            GameTurn().Forget();
        }

        private void OnUndoTurn()
        {
            _step--;

            var prevType = nextElementManager.Undo();
            _boardManager.SetNextElement(prevType);
        }
        
        protected override UniTask OnGameTurn(CancellationToken token)
        {
            IsPlayerTurn = false;
            
            nextElementManager.SaveCurrent();
            
            return UniTask.CompletedTask;
        }
    }
}