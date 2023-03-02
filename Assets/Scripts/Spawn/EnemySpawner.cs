using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnemySpawner : MonoBehaviour , IPauseHandler{

	[SerializeField] private SpawnPatern _spawnPatern;

	private List<Enemy> _spawnedEnemies = new List<Enemy>();

	private float _currentDelay;
	private float _currentMultiplier;
	private Coroutine _spawnWait;
	private float _toNextSpawn;

    private ISpawnPoisition _spawnPoisition;
    private PauseManager _pauseManager;
    private DiContainer _container;
    private int _enemyIndex;

    [Inject]
    private void Construct(ISpawnPoisition spawnPoisition, PauseManager pauseManager, Player player,DiContainer container)
    {
        _spawnPoisition = spawnPoisition;
        _pauseManager = pauseManager;
        _container = container;
        pauseManager.SubscribeHandler(this);
    }

    void Start()
    {
        var container = FindObjectOfType<SceneContext>().Container;
        foreach (EnemyToSpawn enemy in _spawnPatern.enemies)
        {
            foreach (Ability ability in enemy.EnemyStats.Abilities)
            {
                container.Inject(ability);
            }
        }

        _currentDelay = _spawnPatern.StartSpawnDelay;
        _spawnWait = StartCoroutine(SpawnWait(_spawnPatern.DelayBeforeFirstSpawn));
    }

    private void Spawn()
    {
        if (_spawnPatern.enemies.Count < 1)
        {
            Debug.LogError("No enemies in spawn patern! "+ _spawnPatern.name);
        }
        foreach (EnemyToSpawn enemy in _spawnPatern.enemies)
        {
            for (int i = 0; i < GetEnemySpawnCount(enemy); i++)
            {
                var currentEnemy = Instantiate<Enemy>(enemy.EnemyStats.enemy,
                    _spawnPoisition.GetSpawnPosition() + Vector3.up * enemy.EnemyStats.enemy.transform.position.y
                    , Quaternion.identity, this.transform);
                currentEnemy.gameObject.name = enemy.EnemyStats.name + ++_enemyIndex;
                _container.Inject(currentEnemy);
                _spawnedEnemies.Add(currentEnemy);
                currentEnemy.Damaged += OnEnemyDamage;
                currentEnemy.Init(enemy.EnemyStats);
            }
        }
        ChangeSpawnDelay();
        _spawnWait = StartCoroutine(SpawnWait(_currentDelay));
    }

    private int GetEnemySpawnCount(EnemyToSpawn enemyToSpawn)
    {
        if (_currentDelay != _spawnPatern.MinSpawnDelayRate)
        {
            return enemyToSpawn.BaseSpawnCount;
        }
        return enemyToSpawn.BaseSpawnCount + Mathf.FloorToInt(enemyToSpawn.BaseSpawnCount * _currentMultiplier);
    }

    private void ChangeSpawnDelay()
    {
        _currentDelay = Mathf.Max(_currentDelay - _spawnPatern.SpawnDelayChangeStep, _spawnPatern.MinSpawnDelayRate);
        if (_currentDelay == _spawnPatern.MinSpawnDelayRate)
        {
            _currentMultiplier = Mathf.Min(_currentMultiplier + _spawnPatern.EnemyMultplierSpawnRate, _spawnPatern.MaxEnemyMultplierSpawnRate);
        }
    }

    private void OnEnemyDamage(Enemy enemy, HitInfo hit)
    {
        enemy.CurrentHeals -= hit.HealsDamage;

        if (enemy.CurrentHeals < 1)
        {
            DestroyEnemy(enemy);
        }
    }

    private void DestroyEnemy(Enemy enemy)
    {
        _spawnedEnemies.Remove(enemy);
        enemy.Damaged -= OnEnemyDamage;
        Destroy(enemy.gameObject);
    }

    public void SelfDestroy(Enemy enemy)
    {
        _spawnedEnemies.Remove(enemy);
        enemy.Damaged -= OnEnemyDamage;
        Destroy(enemy.gameObject);
    }

    public void OnPause(bool IsPause)
    {
        if (IsPause && _spawnWait != null)
        {
			StopCoroutine(_spawnWait);
        }
        else
        {
			_spawnWait = StartCoroutine(SpawnWait(_toNextSpawn));
        }
    }


	private IEnumerator SpawnWait(float waitTime)
    {
		_toNextSpawn = waitTime;
        while (_toNextSpawn > 0)
        {
			yield return null;
			_toNextSpawn -= Time.deltaTime;
        }

        Spawn();
    }

    private void OnDestroy()
    {
        _pauseManager.UnsubscribeHandler(this);
    }

}
