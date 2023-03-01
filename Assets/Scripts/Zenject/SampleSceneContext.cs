using UnityEngine;
using Zenject;

public class SampleSceneContext : MonoInstaller{

    public override void InstallBindings()
    {
#if UNITY_EDITOR
        Container.Bind<IPlayerInput>().To<KeyBoardInput>().FromComponentInHierarchy().AsSingle();
#else
        Container.Bind<IPlayerInput>().To<UiStickInput>().FromComponentInHierarchy().AsSingle();
#endif

        Container.Bind<ISpawnPoisition>().To<SpawnByRandomRadius>().AsSingle();
        Container.Bind<EnemySpawner>().FromComponentInHierarchy().AsSingle();
        Container.Bind<BulletContainer>().FromComponentInHierarchy().AsSingle();
        Container.Bind<CoroutineRunner>().FromComponentInHierarchy().AsSingle();
        Container.Bind<Player>().FromComponentInHierarchy().AsSingle();

       // Container.Bind<Shooter>().FromInstance(_shooter).AsSingle();
    }
}
