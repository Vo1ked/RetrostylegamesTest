using System;
using System.Collections;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "Shooter", menuName = "My Game/Ability/Shooter")]
public class Shooter : Ability, IAttackAbillity
{
    [Space]
    [SerializeField] private BulletsController Bullet;
    [SerializeField] private float ReloadTime;

    float IAttackAbillity.ReloadTime => ReloadTime;

    [Inject]
    private void Construct(DiContainer container)
    {
        container.Inject(Bullet);

        Specialization = Specialization.Attack;
        WorkType = WorkType.@override;
    }

    public override void Execute(GameObject user, params object[] parameters)
    {
        Bullet.Shooter = user;
        Bullet.Spawn();
    }




}
