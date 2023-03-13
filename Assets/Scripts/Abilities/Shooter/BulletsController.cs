using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System;
using System.Threading.Tasks;
using System.Threading;

[Serializable]
[CreateAssetMenu(fileName = "BulletsController", menuName = "My Game/Shooter/BulletsController")]
public class BulletsController : ScriptableObject, IPauseHandler, IDisposable
{
    [SerializeField] protected BulletsStats _bulletsStats;
    [SerializeField] private Vector3 _spawnOffset;

    [NonSerialized] protected List<Bullet> _spawnedBullets = new List<Bullet>();
    protected static float _autoDeleteTimer = 60;
    [NonSerialized] protected int _bulletIndex;

    [NonSerialized] protected BulletContainer _bulletContainer;
    [NonSerialized] protected PauseManager _pauseManager;

    [Inject]
    private void Construct(BulletContainer bulletContainer, PauseManager pauseManager, DisposeOnSceneExit dispose)
    {
        _bulletContainer = bulletContainer;
        _pauseManager = pauseManager;
        _pauseManager.SubscribeHandler(this);
    }

    public virtual void Spawn(GameObject shooter)
    {
        if (_bulletsStats.Bullet == null)
        {
            Debug.LogError($"Bullet {_bulletsStats.name} did`t contain  bullet");
            return;
        }
        var spawnPosition = shooter.GetComponent<IBulletSpawn>().GetSpawnPosition();
        var _spawnedBullet = GameObject.Instantiate<Bullet>(_bulletsStats.Bullet, spawnPosition + _spawnOffset, Quaternion.identity, _bulletContainer.transform);
        _spawnedBullet.name = $"{_bulletsStats.name}_{shooter.name}({++_bulletIndex})";
        _spawnedBullet.Hited += OnCollision;
        _spawnedBullet.TimeToDeleteLeft = _autoDeleteTimer;
        _spawnedBullet.Shooter = shooter;
        _spawnedBullets.Add(_spawnedBullet);
        //Debug.LogError($"Bullet {_spawnedBullet.name} Shotter = {shooter.name} Spawn position {spawnPosition} _bulletContainer = {_spawnedBullet.transform.parent.name} ");

        Move(_spawnedBullet);
    }

    public virtual void Move(Bullet bullet)
    {
        bullet.transform.LookAt(bullet.Shooter.transform);
        bullet.DestroyCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_pauseManager.PauseCancellationToken.Token);
        MoveForward(bullet, bullet.DestroyCancellationToken.Token);
    }
    public virtual void Dispose()
    {
        _pauseManager.UnsubscribeHandler(this);
    }

    public virtual void OnPause(bool IsPause)
    {
        if (_spawnedBullets.Count < 1)
            return;

        if (!IsPause)
        {
            foreach (Bullet bullet in _spawnedBullets)
            {
                bullet.DestroyCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_pauseManager.PauseCancellationToken.Token);
                MoveForward(bullet, bullet.DestroyCancellationToken.Token);
            }
        }
    }

    protected virtual void OnCollision(Bullet bullet, Collider other)
    {
        var hit = other.GetComponent<IDamageble>();
        if (hit == null)
        {
            Destroy(bullet);
        }
        else
        {
            Hit(bullet, hit);
        }
    }

    protected virtual void Hit(Bullet bullet, IDamageble damageble)
    {
        damageble.Damage(_bulletsStats.HitInfo);
        Destroy(bullet);
    }

    protected virtual void Destroy(Bullet bullet)
    {
        bullet.Hited -= OnCollision;
        _spawnedBullets.Remove(bullet);
        bullet.DestroyCancellationToken.Cancel();
        bullet.DestroyCancellationToken.Dispose();
        GameObject.Destroy(bullet.gameObject);
    }

    protected async void MoveForward(Bullet bullet, CancellationToken token)
    {
        if (bullet == null)
        {
            return;
        }
        var rigidBody = bullet.GetComponent<Rigidbody>();
        while (bullet.TimeToDeleteLeft > 0)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }
            rigidBody.MovePosition(bullet.transform.position + -bullet.transform.forward * _bulletsStats.Speed * Time.deltaTime);
            try
            {
                await Task.Delay(Mathf.RoundToInt(Time.fixedDeltaTime * 1000f), token);
                bullet.TimeToDeleteLeft -= Time.deltaTime;
            }
            catch (TaskCanceledException) { }
        }
    }
}
