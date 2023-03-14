using RetroStyleGamesTest.Pause;
using RetroStyleGamesTest.Data;
using RetroStyleGamesTest.Abillity;

using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace RetroStyleGamesTest.Units
{
    public class Enemy : MonoBehaviour, IDamageble, IBulletSpawn, IPauseHandler, IMovable, IDestroyable
    {
        public event System.Action<Enemy, HitInfo> Damaged = (Enemy, HitInfo) => { };
        public int CurrentHeals;
        public EnemyStats EnemyStats { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
        public Vector3 TargetPosition { get; set; }
        public System.Action CompletedMove { get; set; } = () => { };
        public CancellationTokenSource DestroyCancellationToken { get; set; }

        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private Transform _bulletSpawnPoint;

        private float _attackReloadTimeLeft;
        private IAttackAbillity _attackAbillity;

        private PauseManager _pauseManager;
        private Player _player;
        [Inject]
        private void Construct(PauseManager pauseManager, Player player)
        {
            _pauseManager = pauseManager;
            _player = player;
            pauseManager.SubscribeHandler(this);
        }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
        }

        public void Damage(HitInfo hit)
        {
            Damaged.Invoke(this, hit);
        }

        public Vector3 GetSpawnPosition()
        {
            return _bulletSpawnPoint.position;
        }

        public void Init(EnemyStats stats)
        {
            EnemyStats = stats;
            CurrentHeals = EnemyStats.StartHeals;
            DestroyCancellationToken = CancellationTokenSource
                .CreateLinkedTokenSource(_pauseManager.PauseCancellationToken.Token);

            List<Ability> abilities = new List<Ability>();
            if (!Ability.AbilityCheck(EnemyStats.Abilities, Specialization.Spawn, ref abilities))
            {
                _agent.speed = EnemyStats.MoveSpeed;
                _agent.angularSpeed = EnemyStats.RotationSpeed;

                StartMove();
                Attack();
                return;
            }
            var overrideAbility = abilities.First(abbility => abbility.WorkType == WorkType.@override);

            if (overrideAbility != null)
            {
                CompletedMove += AfterCustomSpawnMove;
                overrideAbility.Execute(gameObject);
            }
        }

        public void OnPause(bool isPause)
        {
            if (_agent.enabled)
            {
                _agent.isStopped = isPause;
            }

            if (isPause && Rigidbody != null)
            {
                Rigidbody.Sleep();
            }

            if (!isPause)
            {
                DestroyCancellationToken = CancellationTokenSource
                    .CreateLinkedTokenSource(_pauseManager.PauseCancellationToken.Token);
                Move(DestroyCancellationToken.Token);

                if (_attackReloadTimeLeft > 0)
                {
                    WaitAttackRange(_attackReloadTimeLeft, DestroyCancellationToken.Token);
                }
            }
        }

        public void StartMove()
        {
            if (DestroyCancellationToken.Token.IsCancellationRequested)
                return;
            Move(DestroyCancellationToken.Token);
        }

        private void AfterCustomSpawnMove()
        {
            StartMove();
            Attack();
            CompletedMove -= AfterCustomSpawnMove;
        }

        private async void Move(CancellationToken token)
        {
            List<Ability> abilities = new List<Ability>();
            if (!Ability.AbilityCheck(EnemyStats.Abilities, Specialization.Move, ref abilities))
            {
                var pathUpdateDelay = 500;
                _agent.SetDestination(_player.transform.position);
                try
                {
                    await Task.Delay(pathUpdateDelay, token);
                }
                catch (TaskCanceledException) { }
                if (token.IsCancellationRequested)
                {
                    return;
                }
                Move(token);
                return;
            }
            var overrideAbility = abilities.First(abbility => abbility.WorkType == WorkType.@override);

            if (overrideAbility != null)
            {
                overrideAbility.Execute(gameObject);
            }
        }

        private void Attack()
        {
            List<Ability> abilities = new List<Ability>();
            if (!Ability.AbilityCheck(EnemyStats.Abilities, Specialization.Attack, ref abilities))
            {
                return;
            }
            var overrideAbility = abilities.First(abbility => abbility.WorkType == WorkType.@override);

            if (overrideAbility != null)
            {
                var attackAbillity = GetAttackAbillity(overrideAbility);
                if (_attackReloadTimeLeft > 0)
                    return;
                var distance = Vector3.Distance(_player.transform.position, transform.position);
                if (distance > attackAbillity.AttackRange)
                {
                    WaitAttackRange(attackAbillity.ReloadTime, DestroyCancellationToken.Token);
                    return;
                }
                overrideAbility.Execute(gameObject, this);
                WaitAttackRange(attackAbillity.ReloadTime, DestroyCancellationToken.Token);
            }
        }

        private IAttackAbillity GetAttackAbillity(Ability ability)
        {
            if (_attackAbillity != null)
                return _attackAbillity;

            return _attackAbillity = ability as IAttackAbillity;
        }

        private async void WaitAttackRange(float timer, CancellationToken token)
        {
            _attackReloadTimeLeft = timer;

            try
            {
                while (_attackReloadTimeLeft > 0)
                {
                    await Task.Delay(Mathf.RoundToInt(Time.fixedDeltaTime * 1000f), token);

                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    _attackReloadTimeLeft -= Time.fixedDeltaTime;
                }

                while (Vector3.Distance(_player.transform.position, transform.position) > _attackAbillity.AttackRange)
                {
                    await Task.Delay(250, token);
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                }
                Attack();
            }
            catch (TaskCanceledException) { }
        }

        private void OnDestroy()
        {
            _pauseManager.UnsubscribeHandler(this);
            DestroyCancellationToken.Cancel();
            DestroyCancellationToken.Dispose();
            CompletedMove -= AfterCustomSpawnMove;
            CompletedMove.Invoke();
        }
    }
}