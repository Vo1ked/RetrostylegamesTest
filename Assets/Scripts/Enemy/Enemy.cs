using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class Enemy : MonoBehaviour, IDamageble , IBulletSpawn , IPauseHandler
{
    public event System.Action<Enemy, HitInfo> Damaged = (Enemy, HitInfo) => { };
    public int CurrentHeals;

    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Transform _bulletSpawnPoint;

    private Coroutine _moveCoroutine;
    private EnemyStats _enemyStats;

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

    public void Damage(HitInfo hit)
    {
        Damaged.Invoke(this, hit);
    }

    public Vector3 GetSpawnPosition()
    {
        Debug.LogError($"GetSpawnPosition {_bulletSpawnPoint.position} Enemy {name}");
        return _bulletSpawnPoint.position;
    }

    public void Init(EnemyStats stats)
    {
        _enemyStats =  stats;
        _agent.speed = _enemyStats.MoveSpeed;
        _agent.angularSpeed = _enemyStats.RotationSpeed;
        CurrentHeals = _enemyStats.StartHeals;

        StartMove();
        Attack();
    }

    public void StartMove()
    {
        _moveCoroutine = StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        var pathUpdateDelay = 0.5f;
        var wait = new WaitForSeconds(pathUpdateDelay);
        _agent.SetDestination(_player.transform.position);
        yield return wait;
        _moveCoroutine = StartCoroutine(Move());
    }
    public void OnPause(bool isPause)
    {
        if (isPause)
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }

            if (_reloadCoroutine != null)
            {
                StopCoroutine(_reloadCoroutine);
                _reloadCoroutine = null;
            }
        }
        else
        {
            _moveCoroutine = StartCoroutine(Move());
            if (_attackReloadTimeLeft > 0)
            {
                _reloadCoroutine = StartCoroutine(WaitAttackRange(_attackReloadTimeLeft));
            }

        }
        if (_agent != null)
        {
            _agent.isStopped = isPause;
        }
    }

    private void Attack()
    {
        List<Ability> abilities = new List<Ability>();
        if (!Ability.AbilityCheck(_enemyStats.Abilities, Specialization.Attack, ref abilities))
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

            abilities.Remove(overrideAbility);
        }

        foreach (Ability ability in abilities)
        {
            ability.Execute(gameObject, this);
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

        if (_attackReloadTimeLeft > 0)
        {
            yield return null;
            _attackReloadTimeLeft -= Time.deltaTime;
        }

        var wait = new WaitForSeconds(0.5f);
        while (Vector3.Distance(_player.transform.position, transform.position) > _attackAbillity.AttackRange)
        {
            yield return wait;
        }
        Attack();
    }

    private void OnDestroy()
    {
        _pauseManager.UnsubscribeHandler(this);
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }

        if (_reloadCoroutine != null)
        {
            StopCoroutine(_reloadCoroutine);
        }

    }
}
