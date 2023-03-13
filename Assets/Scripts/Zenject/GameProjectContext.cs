using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameProjectContext : MonoInstaller {
    [SerializeField] private PlayerStats _playerStats;
    public override void InstallBindings()
    {
        Container.Bind<PlayerStats>().FromInstance(_playerStats).AsSingle();
    }
}
