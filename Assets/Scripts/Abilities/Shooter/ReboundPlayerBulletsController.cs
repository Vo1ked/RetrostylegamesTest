using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject; 

[CreateAssetMenu(fileName = "ReboundPlayerBulletsController", menuName = "My Game/Shooter/ReboundPlayerBulletsController")]
public class ReboundPlayerBulletsController : BulletsController
{

    [SerializeField] private float _baseReboundChance;
    [SerializeField] private float _addingStep;
    [SerializeField] private float _missingHealsPercentForStep;
    [SerializeField] private float _healOnRebountChance;
    [SerializeField] private HitInfo _regenOnReboundHit;

    private List<Bullet> _reboundedBullets = new List<Bullet>();


    private Player _player;
    private EnemySpawner _enemySpawner;
    private PlayerStats _playerStats;
    [Inject]
    private void Construct(Player player, EnemySpawner enemySpawner, PlayerStats playerStats)
    {
        _enemySpawner = enemySpawner;
        _player = player;
        _playerStats = playerStats;
        _player.OnPlayerDestroy += OnPlayerDestroy;
    }

    private void OnPlayerDestroy()
    {
        _player.OnPlayerDestroy -= OnPlayerDestroy;
        _reboundedBullets.Clear();
        _spawnedBullets.Clear();
    }

    protected override void Hit(Bullet bullet, IDamageble damageble)
    {
        damageble.Damage(_bulletsStats.HitInfo);
        Debug.Log("_bulletsStats.HitInfo" + _bulletsStats.HitInfo.HealsDamage);
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

            bullet.transform.LookAt(enemy.transform);
        }
    }
}
