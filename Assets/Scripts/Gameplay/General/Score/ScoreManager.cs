using System.Collections.Generic;
using Gameplay.IOS.CurrencyRelated;
using PT.Tools.EventListener;
using UniRx;
using UnityEngine;
using Zenject;

namespace Gameplay.General.Score
{
    public class ScoreManager : MonoBehaviourEventListener
    {
        public ReactiveProperty<int> TotalScoreReactive { get; private set; } = new();
        public ReactiveProperty<int> BestScoreReactive { get; private set; } = new();

        [Inject] private ScoreMultiplier _multiplier;
        [Inject] private CurrencyManager _currencyManager;

        private Stack<int> _prevScores = new();
        private int _bestScore;

        private int BestScore
        {
            get => _bestScore;
            set
            {
                _bestScore = value;
                BestScoreReactive.Value = value;
                PlayerPrefs.SetInt("BestScore", value);
            }
        }

        private void Awake()
        {
            AddEventActions(new()
            {
                { GlobalEventEnum.GameStarted, OnGameStarted },
                { GlobalEventEnum.GameEnded, OnGameFinished },
                { GlobalEventEnum.GameTurn, OnGameTurn },
                { GlobalEventEnum.UndoTurn, OnUndoTurn },
            });
        }

        private void Start()
        {
            _bestScore = PlayerPrefs.GetInt("BestScore", 0);
            BestScoreReactive.Value = _bestScore;
        }

        private void OnGameStarted()
        {
            ResetScore();
        }

        private void OnGameFinished()
        {
            if (TotalScoreReactive.Value > BestScore)
                BestScore = TotalScoreReactive.Value;
        }

        private void OnGameTurn()
        {
            _prevScores.Push(TotalScoreReactive.Value);
        }

        private void OnUndoTurn()
        {
            if (_prevScores.Count == 0) return;
            TotalScoreReactive.Value = _prevScores.Pop();
        }

        public virtual void UpdateScore(int value)
        {
            TotalScoreReactive.Value += Mathf.RoundToInt(value * _multiplier.Current);
        }

        public void MergePush(Vector2 position, int value, int step)
        {
            TotalScoreReactive.Value += value * step;
            
            _currencyManager.Add(CurrencyType.Gold, Mathf.RoundToInt(value * _multiplier.Current));
        }

        private void ResetScore()
        {
            TotalScoreReactive.Value = 0;
            _prevScores.Clear();
        }
    }
}