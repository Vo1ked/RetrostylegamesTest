using RetroStyleGamesTest.UI;
using RetroStyleGamesTest.SpawnPosition;
using RetroStyleGamesTest.Pause;
using RetroStyleGamesTest.Data;
using RetroStyleGamesTest.Units;
using RetroStyleGamesTest.Abillity.Implementation;

using System.Collections.Generic;
using Zenject;

namespace RetroStyleGamesTest.Zenject
{
    public class SampleSceneContext : MonoInstaller
    {

        private DisposeOnSceneExit _disposeOnSceneExit;
        public override void InstallBindings()
        {
#if UNITY_EDITOR
            Container.Bind<IPlayerInput>().To<KeyBoardInput>().FromComponentInHierarchy().AsSingle();
#else
        Container.Bind<IPlayerInput>().To<UiStickInput>().FromComponentInHierarchy().AsSingle();
#endif
            _disposeOnSceneExit = new DisposeOnSceneExit();
            Container.Bind<DisposeOnSceneExit>().FromInstance(_disposeOnSceneExit).AsSingle();
            Container.Bind<SpawnPositionFactory>().AsSingle();
            Container.Bind<EnemySpawner>().FromComponentInHierarchy().AsSingle();
            Container.Bind<BulletContainer>().FromComponentInHierarchy().AsSingle();
            Container.Bind<Player>().FromComponentInHierarchy().AsSingle();
            Container.Bind<UiManager>().FromComponentInHierarchy().AsSingle();
            var pauseManager = new PauseManager();
            Container.Bind<PauseManager>().FromInstance(pauseManager).AsSingle();
            _disposeOnSceneExit.Add(pauseManager);
            Container.Bind<Score>().AsSingle();
        }

        private void OnDestroy()
        {
            _disposeOnSceneExit.Dispose();
        }
    }

    public class DisposeOnSceneExit
    {
        private List<System.IDisposable> _disposables = new List<System.IDisposable>();

        public void Add(System.IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        public void Dispose()
        {
            _disposables.ForEach(x => x.Dispose());
            _disposables.Clear();
        }
    }
}
