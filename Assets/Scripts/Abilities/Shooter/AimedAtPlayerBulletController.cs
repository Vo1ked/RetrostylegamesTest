using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Threading.Tasks;
using System.Threading;

[CreateAssetMenu(fileName = "AimedToPlayerBulletController", menuName = "My Game/Shooter/AimBulletController")]
public class AimedAtPlayerBulletController : BulletsController
{
    [System.NonSerialized] private List<Bullet> _loseTargetBullets = new List<Bullet>();

    [System.NonSerialized] private Player _player;
    [System.NonSerialized] private DisposeOnSceneExit _onSceneExit;

    [Inject]
    private void Construct(Player player, DisposeOnSceneExit onSceneExit)
    {
        _player = player;
        _player.PlayerStartTeleport += OnPlayerTeleported;
        _onSceneExit = onSceneExit;
        _onSceneExit.Add(this);
    }

    public override void Dispose()
    {
        base.Dispose();
        _player.PlayerStartTeleport -= OnPlayerTeleported;
        _loseTargetBullets.Clear();
        _spawnedBullets.Clear();
    }

    public override void Move(Bullet bullet)
    {
        bullet.transform.LookAt(_player.transform);
        bullet.DestroyCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_pauseManager.PauseCancellationToken.Token);
        MoveToPlayer(bullet, bullet.DestroyCancellationToken.Token);
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

    private async void MoveToPlayer(Bullet bullet, CancellationToken token)
    {
        var rigidBody = bullet.GetComponent<Rigidbody>();
        try
        {
            while (bullet.TimeToDeleteLeft > 0)
            {
                if (token.IsCancellationRequested)
                    return;

                var direction = (_player.transform.position - bullet.transform.position).normalized;
                rigidBody.MovePosition(bullet.transform.position + direction
                    * _bulletsStats.Speed * Time.deltaTime);
                bullet.TimeToDeleteLeft -= Time.deltaTime;

                await Task.Delay(Mathf.RoundToInt(Time.fixedDeltaTime * 1000f), token);
            }
        }
        catch (TaskCanceledException) { }
    }
}
