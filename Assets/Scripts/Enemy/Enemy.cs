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
                _reloadCoroutine = StartCoroutine(Reload(_attackReloadTimeLeft));
            }

        }
        if (_agent != null)
        {
            _agent.isStopped = isPause;
        }
    }

    private void Attack()
    {
        if (_attackReloadTimeLeft > 0)
            return;

        List<Ability> abilities = new List<Ability>();
        if (!Ability.AbilityCheck(_enemyStats.Abilities, Specialization.Attack, ref abilities))
        {
            return;
        }
        var overrideAbility = abilities.First(abbility => abbility.WorkType == WorkType.@override);

        if (overrideAbility != null)
        {
            var attackAbility = (overrideAbility as IAttackAbillity).ReloadTime;
            overrideAbility.Execute(gameObject, this);
            _reloadCoroutine = StartCoroutine(Reload(attackAbility));

            abilities.Remove(overrideAbility);
        }

        foreach (Ability ability in abilities)
        {
            ability.Execute(gameObject, this);
        }
    }

    private IEnumerator Reload(float timer)
    {
        _attackReloadTimeLeft = timer;

        while (_attackReloadTimeLeft > 0)
        {
            yield return null;
            _attackReloadTimeLeft -= Time.deltaTime;
        }
        _reloadCoroutine = null;
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
