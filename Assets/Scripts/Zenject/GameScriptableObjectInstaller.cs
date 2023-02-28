using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "GameScriptableObjectInstaller", menuName = "Installers/GameScriptableObjectInstaller")]
public class GameScriptableObjectInstaller : ScriptableObjectInstaller<GameScriptableObjectInstaller>
{
    [SerializeField] private PlayerStats _playerStats;

    public override void InstallBindings()
    {
        Container.Bind<PlayerStats>().FromInstance(_playerStats).AsSingle();
    }
}