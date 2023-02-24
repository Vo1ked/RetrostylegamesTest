using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SampleSceneContext : MonoInstaller {

    public override void InstallBindings()
    {

       // Container.BindInterfacesTo<UiStickInput>();
        Container.Bind<IPlayerInput>().To<UiStickInput>().FromComponentInHierarchy().AsSingle();

    }
}
