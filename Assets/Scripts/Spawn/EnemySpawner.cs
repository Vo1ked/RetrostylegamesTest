using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Threading;
using System.Threading.Tasks;

public class EnemySpawner : MonoBehaviour , IPauseHandler{

	[SerializeField] private SpawnPatern _spawnPatern;

	private List<Enemy> _spawnedEnemies = new List<Enemy>();

	private float _currentDelay;
	private float _currentMultiplier;
	private float _toNextSpawn;

    private SpawnFactory _spawnPoisition;
    private PauseManager _pauseManager;
    private DiContainer _container;
    private int _enemyIndex;
    private PlayerStats _playerStats;
    private IPlayerInput _input;
    private Score _score;

    [Inject]
    private void Construct(SpawnFactory spawnPoisition, PauseManager pauseManager, PlayerStats playerStats, DiContainer container,
        IPlayerInput input, Score score)
    {
        _spawnPoisition = spawnPoisition;
        _pauseManager = pauseManager;
        _container = container;
        _playerStats = playerStats;
        _input = input;
        _score = score;

        _input.Ultimate += OnUltimate;
        pauseManager.SubscribeHandler(this);
    }

    public Enemy GetNearestEnemy(Vector3 position)
    {
        if (_spawnedEnemies.Count < 1)
        {
            return null;
        }
        var distance = Mathf.Infinity;
        Enemy nearestEnemy = null;
        foreach (Enemy enemy in _spawnedEnemies)
        {
            var currentDistance = Vector3.Distance(position, enemy.transform.position);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                nearestEnemy = enemy;
            }
        }
        return nearestEnemy;
    }

    public void Destroy(Enemy enemy)
    {
        _spawnedEnemies.Remove(enemy);
        enemy.Damaged -= OnEnemyDamage;
        Destroy(enemy.gameObject);
    }
    public Vector3[] GetEnemiesPosition()
    {
        return _spawnedEnemies.ConvertAll(x => x.transform.position).ToArray();
    }

    public void OnPause(bool IsPause)
    {
        if (!IsPause)
        {
            SpawnWait(_toNextSpawn, _pauseManager.PauseCancellationToken.Token);
        }
    }

    private void OnUltimate()
    {
        if (_playerStats.Mana.CurrentMana >= _playerStats.Mana.MaxMana)
        {
            var enemyToDestory = new List<Enemy>(_spawnedEnemies);
            foreach (Enemy enemy in enemyToDestory)
            {
                Destroy(enemy);
                _score.CurrentScore++;
            }
            _playerStats.Mana.CurrentMana = 0;
        }

    }
    private void Start()
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
        SpawnWait(_spawnPatern.DelayBeforeFirstSpawn, _pauseManager.PauseCancellationToken.Token);
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
                    _spawnPoisition.GetSpawnPosition(SpawnType.random) + Vector3.up * enemy.EnemyStats.enemy.transform.position.y
                    , Quaternion.identity, this.transform);
                currentEnemy.gameObject.name = enemy.EnemyStats.name + ++_enemyIndex;
                _container.Inject(currentEnemy);
                _spawnedEnemies.Add(currentEnemy);
                currentEnemy.Damaged += OnEnemyDamage;
                currentEnemy.Init(enemy.EnemyStats);
            }
        }
        ChangeSpawnDelay();
        SpawnWait(_currentDelay,_pauseManager.PauseCancellationToken.Token);
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
        _score.CurrentScore++;
        Destroy(enemy);
    }

	private async void SpawnWait(float waitTime, CancellationToken token)
    {
		_toNextSpawn = waitTime;
		var stopwatch = new  System.Diagnostics.Stopwatch();
        try
        {
            await Task.Delay(Mathf.RoundToInt(waitTime * 1000f), token);
            if (token.IsCancellationRequested)
                return;
            Spawn();
        }
        catch (TaskCanceledException)
        {
            _toNextSpawn = waitTime - (float)stopwatch.Elapsed.TotalSeconds;
        }

        stopwatch.Stop();

    }

    private void OnDestroy()
    {
        _pauseManager.UnsubscribeHandler(this);
        _input.Ultimate -= OnUltimate;
    }

}
