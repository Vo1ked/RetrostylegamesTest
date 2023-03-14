using UnityEngine;
using Zenject;
using RetroStyleGamesTest.Data;

namespace RetroStyleGamesTest.Zenject
{
    public class GameProjectContext : MonoInstaller
    {
        [SerializeField] private PlayerStats _playerStats;
        public override void InstallBindings()
        {
            Container.Bind<PlayerStats>().FromInstance(_playerStats).AsSingle();
        }
    }
}
