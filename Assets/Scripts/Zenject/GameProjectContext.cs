using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameProjectContext : MonoInstaller {
    [SerializeField] private PlayerStats _playerStats;
    private Dispose _dispose;
    public override void InstallBindings()
    {
        Container.Bind<PlayerStats>().FromInstance(_playerStats).AsSingle();
        _dispose = new Dispose();
        Container.Bind<Dispose>().FromInstance(_dispose).AsSingle();
    }

    private void OnDestroy()
    {
        _dispose.OnDispose.Invoke();
        _dispose.OnDispose = null;
    }
}
