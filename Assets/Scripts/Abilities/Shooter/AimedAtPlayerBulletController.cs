using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "AimedToPlayerBulletController", menuName = "My Game/Shooter/AimBulletController")]
public class AimedAtPlayerBulletController : BulletsController
{

    private Player _player;
    [System.NonSerialized] private List<Bullet> _loseTargetBullets = new List<Bullet>();
    [Inject]
    private void Construct(Player player)
    {
        _player = player;
        _player.PlayerStartTeleport += OnPlayerTeleported;
        _player.OnPlayerDestroy += OnPlayerDestroy;
    }

    private void OnPlayerDestroy()
    {
        _player.PlayerStartTeleport -= OnPlayerTeleported;
        _player.OnPlayerDestroy -= OnPlayerDestroy;
        _loseTargetBullets.Clear();
        _spawnedBullets.Clear();
    }

    private void OnPlayerTeleported(Vector3 lastPosition)
    {
        var bulletsToRemove = new List<Bullet>(_spawnedBullets);
        foreach (Bullet bullet in _loseTargetBullets)
        {
            bulletsToRemove.Remove(bullet);
        }

        foreach (Bullet bullet in bulletsToRemove)
        {
            if (bullet.MoveCoroutine != null)
            {
                _coroutineRunner.StopRunningCoroutine(bullet.MoveCoroutine);
                bullet.MoveCoroutine = null;
            }
            _loseTargetBullets.Add(bullet);
            bullet.transform.rotation = Quaternion.LookRotation(bullet.transform.position - _player.transform.position);
            bullet.MoveCoroutine = _coroutineRunner.StartCoroutine(MoveForward(bullet));
        }
    }

    protected override void Destroy(Bullet bullet)
    {
        base.Destroy(bullet);
        _loseTargetBullets.Remove(bullet);
    }

    public override void Move(Bullet bullet)
    {
        bullet.transform.LookAt(_player.transform);
        bullet.MoveCoroutine = _coroutineRunner.RunCoroutine(MoveToPlayer(bullet));
    }

    private IEnumerator MoveToPlayer(Bullet bullet)
    {
        var rigidBody = bullet.GetComponent<Rigidbody>();
        while (bullet.TimeToDeleteLeft > 0)
        {
            if (_pauseManager.IsPaused)
            {
                yield break;
            }
            var direction = (_player.transform.position - bullet.transform.position).normalized;
            rigidBody.MovePosition(bullet.transform.position + direction * _bulletsStats.Speed * Time.deltaTime);
            yield return null;
            bullet.TimeToDeleteLeft -= Time.deltaTime;
        }

        bullet.MoveCoroutine = null;
    }
}
