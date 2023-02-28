using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameProjectContext : MonoInstaller {

    public override void InstallBindings()
    {
        Container.Bind<PauseManager>().AsSingle();
    }

}
