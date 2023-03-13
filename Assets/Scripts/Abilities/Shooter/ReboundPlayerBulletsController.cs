using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject; 

[CreateAssetMenu(fileName = "ReboundPlayerBulletsController", menuName = "My Game/Shooter/ReboundPlayerBulletsController")]
public class ReboundPlayerBulletsController : BulletsController, System.IDisposable
{

    [SerializeField] private float _baseReboundChance;
    [SerializeField] private float _addingStep;
    [SerializeField] private float _missingHealsPercentForStep;
    [SerializeField] private float _healOnRebountChance;
    [SerializeField] private HitInfo _regenOnReboundHit;

    [System.NonSerialized] private List<Bullet> _reboundedBullets = new List<Bullet>();

    [System.NonSerialized] private DisposeOnSceneExit _onSceneExit;
    [System.NonSerialized] private EnemySpawner _enemySpawner;
    [System.NonSerialized] private PlayerStats _playerStats;
    [Inject]
    private void Construct(EnemySpawner enemySpawner, PlayerStats playerStats, DisposeOnSceneExit onSceneExit)
    {
        _onSceneExit = onSceneExit;
        _enemySpawner = enemySpawner;
        _playerStats = playerStats;
        _onSceneExit.Add(this);
    }
    public void Dispose()
    {
        _reboundedBullets.Clear();
        _spawnedBullets.Clear();
    }

    protected override void Hit(Bullet bullet, IDamageble damageble)
    {
        damageble.Damage(_bulletsStats.HitInfo);
        if (_reboundedBullets.Contains(bullet))
        {

            if (Random.value < _healOnRebountChance)
            {
                _playerStats.Heals.CurrentHeals += _regenOnReboundHit.HealsDamage;
            }
            else
            {
                _playerStats.Mana.CurrentMana += _regenOnReboundHit.ManaDamage;
            }
            _reboundedBullets.Remove(bullet);
            Destroy(bullet);
        }

        var reboundChance = _baseReboundChance + _addingStep * ((1 - (float)_playerStats.Heals.CurrentHeals / _playerStats.Heals.MaxHeals) / _missingHealsPercentForStep);
        var random = Random.value;
        if (reboundChance > random)
        {
            _reboundedBullets.Add(bullet);
            var enemy = _enemySpawner.GetNearestEnemy(bullet.transform.position);

            if (enemy == null)
            {
                return;
            }

            bullet.transform.rotation = Quaternion.LookRotation(bullet.transform.position - enemy.transform.position);
        }
    }
}
