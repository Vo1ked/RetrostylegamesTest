using UnityEngine;
using Zenject;

namespace RetroStyleGamesTest.Abillity.Implementation
{
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

        private bool Inited;

        [SerializeField] private BulletsController Bullet;

        [System.NonSerialized] private DiContainer _container;
        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;
        }

        public override void Execute(GameObject user, params object[] parameters)
        {
            TryInit();
            Bullet.Spawn(user);
        }

        private void TryInit()
        {
            if (Inited)
                return;

            _container.Inject(Bullet);
        }
    }
}
