using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Threading.Tasks;
using System.Threading;

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
            _loseTargetBullets.Add(bullet);
            bullet.transform.rotation = Quaternion.LookRotation(bullet.transform.position - _player.transform.position);
            bullet.DestroyCancellationToken.Cancel();
            bullet.DestroyCancellationToken.Dispose();
            bullet.DestroyCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_pauseManager.PauseCancellationToken.Token);
            MoveForward(bullet, bullet.DestroyCancellationToken.Token);
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
        bullet.DestroyCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_pauseManager.PauseCancellationToken.Token);
        MoveToPlayer(bullet, bullet.DestroyCancellationToken.Token);
    }

    private async void MoveToPlayer(Bullet bullet,CancellationToken token)
    {
        var rigidBody = bullet.GetComponent<Rigidbody>();
        while (bullet.TimeToDeleteLeft > 0)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }
            var direction = (_player.transform.position - bullet.transform.position).normalized;
            rigidBody.MovePosition(bullet.transform.position + direction * _bulletsStats.Speed * Time.deltaTime);
            bullet.TimeToDeleteLeft -= Time.deltaTime;
            try
            {
                await Task.Delay(Mathf.RoundToInt(Time.fixedDeltaTime * 1000f), token);
            }
            catch (TaskCanceledException) { }
        }
    }
}
