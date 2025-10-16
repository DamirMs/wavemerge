﻿using Gameplay.Current.Configs;
using PT.Logic.PersistentScene;
using UnityEngine;
using Zenject;

namespace PT.Logic.Dependency.ProjectContext
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private GameInfoConfig gameInfoConfig; 
        
        public override void InstallBindings()
        {
            Container.Bind<SceneLoadManager>().FromComponentInHierarchy().AsSingle();

            Container.Bind<IFactoryZenject>().To<FactoryZenject>().AsSingle();
            
            Container.Bind<GameInfoConfig>().FromInstance(gameInfoConfig).AsSingle();
        }
    }
}