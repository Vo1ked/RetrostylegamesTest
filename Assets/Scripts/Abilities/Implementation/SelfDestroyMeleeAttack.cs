﻿using UnityEngine;
using Zenject;
using RetroStyleGamesTest.Units;

namespace RetroStyleGamesTest.Abillity.Implementation
{
    [CreateAssetMenu(fileName = "SelfDestroyMeleeAttack", menuName = "My Game/Ability/SelfDestroyMeleeAttack")]
    public class SelfDestroyMeleeAttack : Ability, IAttackAbillity
    {
        public override Specialization Specialization => Specialization.Attack;
        public override WorkType WorkType => WorkType.@override;
        public HitInfo HitInfo;

        [SerializeField] private float ReloadTime;
        [SerializeField] private float Range = 2;

        float IAttackAbillity.ReloadTime => ReloadTime;
        float IAttackAbillity.AttackRange => Range;

        private Player _player;
        private EnemySpawner _enemySpawner;
        [Inject]
        private void Construct(EnemySpawner enemySpawner, Player player)
        {
            _player = player;
            _enemySpawner = enemySpawner;
        }

        public override void Execute(GameObject user, params object[] parameters)
        {
            RaycastHit hit;
            if (Physics.Raycast(new Ray(user.transform.position, _player.transform.position - user.transform.position), out hit, Range))
            {
                var damageble = hit.transform.GetComponent<IDamageble>();
                if (damageble != null)
                {
                    damageble.Damage(HitInfo);
                    _enemySpawner.Destroy(user.GetComponent<Enemy>());
                }
            }
        }
    }
}
