using System.Collections;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "AimedToPlayerBulletController", menuName = "My Game/Shooter/AimBulletController")]
public class AimedAtPlayerBulletController : BulletsController {

    private Player _player; 
    [Inject]
    private void Construct(Player player)
    {
        _player = player;
        _player.PlayerStartTeleport += OnPlayerTeleported;
    }

    private void OnPlayerTeleported(Vector3 lastPosition)
    {
        foreach (Bullet bullet in _spawnedBullets)
        {
            _coroutineRunner.StopCoroutine(bullet.MoveCoroutine);
            bullet.MoveCoroutine = _coroutineRunner.StartCoroutine(MoveForward(bullet));
        }
    }

    public override void Move(Bullet bullet)
    {
        bullet.transform.LookAt(_player.transform);
        bullet.MoveCoroutine = _coroutineRunner.StartCoroutine(MoveToPlayer(bullet));
    }

    private IEnumerator MoveToPlayer(Bullet bullet)
    {
        var rigidBody = bullet.GetComponent<Rigidbody>();
        while (bullet.TimeToDeleteLeft > 0)
        {
            var direction = (_player.transform.position - bullet.transform.position).normalized;
            rigidBody.MovePosition(bullet.transform.position + direction * _bulletsStats.Speed * Time.deltaTime);
            yield return null;
            bullet.TimeToDeleteLeft -= Time.deltaTime;
        }

        bullet.MoveCoroutine = null;
    }
}
