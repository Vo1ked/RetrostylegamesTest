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
    private PlayerStats _playerStats;
    private IPlayerInput _input;
    [Inject]
    private void Construct(ISpawnPoisition spawnPoisition, PauseManager pauseManager, PlayerStats playerStats, DiContainer container,
        IPlayerInput input)
    {
        _spawnPoisition = spawnPoisition;
        _pauseManager = pauseManager;
        _container = container;
        _playerStats = playerStats;
        _input = input;
        _input.Ultimate += OnUltimate;
        pauseManager.SubscribeHandler(this);
    }

    private void OnUltimate()
    {
        if (_playerStats.Mana.CurrentMana >= _playerStats.Mana.MaxMana)
        {
            var enemyToDestory = new List<Enemy>(_spawnedEnemies);
            foreach (Enemy enemy in enemyToDestory)
            {
                Destroy(enemy);
            }
            _playerStats.Mana.CurrentMana = 0;
        }

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
            OnEnemyKIll(enemy);
        }
    }

    private void OnEnemyKIll(Enemy enemy)
    {
        _playerStats.Mana.CurrentMana += enemy.EnemyStats.ManaAtkill;
        Destroy(enemy);
    }

    public void Destroy(Enemy enemy)
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
