using Gameplay.Current.Shockwave2048.Elements;
using UnityEngine;

namespace Gameplay.Current.Shockwave2048
{
    [CreateAssetMenu(menuName = "Configs/ElementsConfig", fileName = "ElementsConfig")]
    public class ElementsConfig : ScriptableObject
    {
        [SerializeField] private ElementData[] elementDatas;
        
        public ElementData[] ElementDatas => elementDatas;
    }
}