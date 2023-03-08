using System.Collections.Generic;
using Zenject;

public class SampleSceneContext : MonoInstaller{

    private List<System.IDisposable> _disposables = new List<System.IDisposable>();

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
        Container.Bind<UiManager>().FromComponentInHierarchy().AsSingle();

        var pauseManager = new PauseManager();
        _disposables.Add(pauseManager);
        Container.Bind<PauseManager>().FromInstance(pauseManager).AsSingle();
        Container.Bind<Score>().AsSingle();

    }

    private void OnDestroy()
    {
        _disposables.ForEach(x => x.Dispose());
    }
}
