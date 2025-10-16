using Gameplay.Current.Shockwave2048.Enums;
using Gameplay.General.Configs;
using UnityEngine;

namespace Gameplay.Current.Configs
{
    [CreateAssetMenu(menuName = "Configs/GameInfo", fileName = "GameInfoConfig")]
    public class GameInfoConfig : BaseGameInfoConfig
    {
        [Space(20)]
        [Header("GAME settings:")]
        [SerializeField] private int gridWidth = 5;
        [SerializeField] private int elementWidth = 120;
        [SerializeField] private ElementType[] playableElementTypes;
        [Header("Animations")]
        [SerializeField] private float[] elementSpeedComboProgression;
        [SerializeField] private float shockwavePushDuration = 1;
        [SerializeField] private float shockwavePushSize = 3;
        
        public int GridWidth => gridWidth;
        public int ElementWidth => elementWidth;
        public ElementType[] PlayableElementTypes => playableElementTypes;
        public float[] ElementSpeedComboProgression => elementSpeedComboProgression;
        public float ShockwavePushDuration => shockwavePushDuration;
        public float ShockwavePushSize => shockwavePushSize;
    }
}