using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class Enemy : MonoBehaviour, IDamageble , IBulletSpawn , IPauseHandler, IAttack
{
    public event System.Action<Enemy, HitInfo> Damaged = (Enemy, HitInfo) => { };
    public int CurrentHeals;
    public EnemyStats _enemyStats;

    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Transform _bulletSpawnPoint;

    private Coroutine _moveCoroutine;


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
        _enemyStats = stats;
        name = _enemyStats.name;
        CurrentHeals = _enemyStats.StartHeals;
        _agent.speed = _enemyStats.MoveSpeed;
        _agent.angularSpeed = _enemyStats.RotationSpeed;

        StartMove();
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
            if (_moveCoroutine == null)
                return;

            StopCoroutine(_moveCoroutine);
            _agent.isStopped = true;
            _moveCoroutine = null;
        }
        else
        {
            _agent.isStopped = false;
            _moveCoroutine = StartCoroutine(Move());
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
            overrideAbility.Execute(gameObject, this);
            abilities.Remove(overrideAbility);
        }

        foreach (Ability ability in abilities)
        {
            ability.Execute(gameObject, this);
        }

    }

    public void TryAttack()
    {
        Attack();
    }

    private void OnDestroy()
    {
        _pauseManager.UnsubscribeHandler(this);
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }

    }
}
