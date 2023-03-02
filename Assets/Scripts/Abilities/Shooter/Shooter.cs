using System;
using System.Collections;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "Shooter", menuName = "My Game/Ability/Shooter")]
public class Shooter : Ability, IAttackAbillity
{
    public override Specialization Specialization => Specialization.Attack;
    public override WorkType WorkType => WorkType.@override;
    [Space]

    [SerializeField] private float ReloadTime;
    [SerializeField] private float Range = 9999;

    float IAttackAbillity.ReloadTime => ReloadTime;
    float IAttackAbillity.AttackRange => Range;
    

    [SerializeField] private BulletsController Bullet;

    [Inject]
    private void Construct(DiContainer container)
    {
        container.Inject(Bullet);
    }

    public override void Execute(GameObject user, params object[] parameters)
    {
        Bullet.Spawn(user);
    }




}
