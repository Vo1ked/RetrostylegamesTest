using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class Enemy : MonoBehaviour, IDamageble , IBulletSpawn , IPauseHandler, IMovable
{
    public event System.Action<Enemy, HitInfo> Damaged = (Enemy, HitInfo) => { };
    public int CurrentHeals;
    public EnemyStats EnemyStats { get; private set; }

    public Coroutine Coroutine { get; set; }
    public Rigidbody Rigidbody { get; private set; }
    public Vector3 TargetPosition { get; set; }
    public System.Action CompletedMove { get; set; }

    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Transform _bulletSpawnPoint;

    private float _attackReloadTimeLeft;
    private Coroutine _reloadCoroutine;
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

    private void AfterCustomSpawnMove()
    {
        StartMove();
        Attack();
        CompletedMove -= AfterCustomSpawnMove;
    }

    public void StartMove()
    {
        Coroutine = StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        List<Ability> abilities = new List<Ability>();
        if (!Ability.AbilityCheck(EnemyStats.Abilities, Specialization.Move, ref abilities))
        {
            var pathUpdateDelay = 0.5f;
            var wait = new WaitForSeconds(pathUpdateDelay);
            _agent.SetDestination(_player.transform.position);
            yield return wait;
            if (_pauseManager.IsPaused)
            {
                yield break;
            }
            if (Coroutine != null)
            {
                StopCoroutine(Coroutine);
                Coroutine = null;
            }
            Coroutine = StartCoroutine(Move());
            yield break;
        }
        var overrideAbility = abilities.First(abbility => abbility.WorkType == WorkType.@override);

        if (overrideAbility != null)
        {
            overrideAbility.Execute(gameObject, this, Coroutine);
        }
    }
    public void OnPause(bool isPause)
    {
        if (_agent.enabled)
        {
            _agent.isStopped = isPause;
        }

        if (isPause)
        {
            if (Coroutine != null)
            {
                StopCoroutine(Coroutine);
                Coroutine = null;
            }

            if (_reloadCoroutine != null)
            {
                StopCoroutine(_reloadCoroutine);
                _reloadCoroutine = null;
            }
        }
        else
        {
            if (Coroutine == null)
            {
                Coroutine = StartCoroutine(Move());
            }
            if (_attackReloadTimeLeft > 0 && _reloadCoroutine == null)
            {
                _reloadCoroutine = StartCoroutine(WaitAttackRange(_attackReloadTimeLeft));
            }
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

            if (distance > attackAbillity.AttackRange && _reloadCoroutine == null)
            {
                _reloadCoroutine = StartCoroutine(WaitAttackRange(attackAbillity.ReloadTime));
                return;
            }
            overrideAbility.Execute(gameObject, this);
            _reloadCoroutine = StartCoroutine(WaitAttackRange(attackAbillity.ReloadTime));
        }
    }

    private IAttackAbillity GetAttackAbillity(Ability ability)
    {
        if (_attackAbillity != null)
            return _attackAbillity;

        
        return _attackAbillity = ability as IAttackAbillity;
    }

    private IEnumerator WaitAttackRange(float timer)
    {
        _attackReloadTimeLeft = timer;

        while (_attackReloadTimeLeft > 0)
        {
            yield return null;
            if (_pauseManager.IsPaused)
            {
                yield break;
            }
            _attackReloadTimeLeft -= Time.deltaTime;
        }

        var wait = new WaitForSeconds(0.5f);
        while (Vector3.Distance(_player.transform.position, transform.position) > _attackAbillity.AttackRange)
        {
            yield return wait;
            if (_pauseManager.IsPaused)
            {
                yield break;
            }
        }
        _reloadCoroutine = null;
        Attack();
    }

    private void OnDestroy()
    {
        _pauseManager.UnsubscribeHandler(this);
        if (Coroutine != null)
        {
            StopCoroutine(Coroutine);
            Coroutine = null;
        }

        if (_reloadCoroutine != null)
        {
            StopCoroutine(_reloadCoroutine);
            _reloadCoroutine = null;

        }

    }
}
