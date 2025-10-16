using Gameplay.Current.Shockwave2048;
using Gameplay.Current.Shockwave2048.Board;
using Gameplay.Current.Shockwave2048.Elements;
using Gameplay.General.Installers;
using UnityEngine;

namespace Gameplay.Current.Installers
{
    public class GameInstaller : BaseGameInstaller
    {
        [SerializeField] private ElementsConfig elementsConfig; 
        
        public override void InstallBindings()
        {
            base.InstallBindings();
            
            Container.Bind<GameplayController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ShockwaveProcessor>().FromComponentInHierarchy().AsSingle();
            Container.Bind<BoardManager>().FromComponentInHierarchy().AsSingle();
            
            Container.Bind<ElementsConfig>().FromInstance(elementsConfig).AsSingle();
            Container.BindInterfacesAndSelfTo<ElementProvider>().AsSingle();
        }
    }
}