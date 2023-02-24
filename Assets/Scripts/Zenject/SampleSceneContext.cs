using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SampleSceneContext : MonoInstaller {

    public override void InstallBindings()
    {
#if UNITY_EDITOR
        Container.Bind<IPlayerInput>().To<KeyBoardInput>().FromComponentInHierarchy().AsSingle();
#else
        Container.Bind<IPlayerInput>().To<UiStickInput>().FromComponentInHierarchy().AsSingle();
#endif

    }
}
