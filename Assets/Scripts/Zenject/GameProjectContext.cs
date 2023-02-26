using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameProjectContext : MonoInstaller {

    [SerializeField] private PlayerStats playerStats;

    public override void InstallBindings()
    {
        Container.Bind<PlayerStats>()
            .FromInstance(playerStats)
            .AsSingle()
            .NonLazy();

        Container.Bind<PauseManager>().FromNew().AsSingle();
        
    }

}
