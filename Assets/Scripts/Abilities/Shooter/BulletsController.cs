using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System;

[Serializable]
[CreateAssetMenu(fileName = "BulletsController", menuName = "My Game/Shooter/BulletsController")]
public class BulletsController : ScriptableObject, IPauseHandler
{
    [SerializeField] private Vector3 _spawnOffset;

    [SerializeField] protected BulletsStats _bulletsStats;
    [NonSerialized] protected List<Bullet> _spawnedBullets = new List<Bullet>();
    protected static float _autoDeleteTimer = 60;
    [NonSerialized] protected int _bulletIndex;

    protected BulletContainer _bulletContainer;
    protected CoroutineRunner _coroutineRunner;
    protected PauseManager _pauseManager;

    [Inject]
    private void Construct(BulletContainer bulletContainer, CoroutineRunner coroutineRunner, PauseManager pauseManager, Dispose dispose)
    {
        _bulletContainer = bulletContainer;
        _coroutineRunner = coroutineRunner;
        _pauseManager = pauseManager;
        _pauseManager.SubscribeHandler(this);

        dispose.OnDispose += () => _pauseManager.UnsubscribeHandler(this);

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

    protected virtual void OnCollision(Bullet bullet,Collider other)
    {
        var hit = other.GetComponent<IDamageble>();
        if (hit == null)
        {
            Destroy(bullet);
        }
        else
        {
            Hit(bullet,hit);
        }
    }

    public virtual void Move(Bullet bullet)
    {
        bullet.transform.LookAt(bullet.Shooter.transform);
        bullet.MoveCoroutine = _coroutineRunner.RunCoroutine(MoveForward(bullet));
    }


    protected virtual void Hit(Bullet bullet,IDamageble damageble)
    {
        damageble.Damage(_bulletsStats.HitInfo);
        Destroy(bullet);
    }

    protected virtual void Destroy(Bullet bullet)
    {
        bullet.Hited -= OnCollision;
        if (bullet.MoveCoroutine != null)
        {
            _coroutineRunner.StopRunningCoroutine(bullet.MoveCoroutine);
            bullet.MoveCoroutine = null;
        }

        _spawnedBullets.Remove(bullet);
        GameObject.Destroy(bullet.gameObject);
    }

    protected IEnumerator MoveForward(Bullet bullet)
    {
        if (bullet == null)
        {
            yield break;
        }
        var rigidBody = bullet.GetComponent<Rigidbody>();
        while (bullet.TimeToDeleteLeft > 0)
        {
            if (_pauseManager.IsPaused)
            {
                yield break;
            }
            rigidBody.MovePosition(bullet.transform.position + -bullet.transform.forward * _bulletsStats.Speed * Time.deltaTime);
            yield return null;
            bullet.TimeToDeleteLeft -= Time.deltaTime;

        }

        bullet.MoveCoroutine = null;
    }

    public void OnPause(bool IsPause)
    {
        if (IsPause)
        {
            if (_spawnedBullets.Count < 1)
                return;

            foreach (Bullet bullet in _spawnedBullets)
            {
                if (bullet.MoveCoroutine != null)
                {
                    _coroutineRunner.StopRunningCoroutine(bullet.MoveCoroutine);
                    bullet.MoveCoroutine = null;
                }
            }
        }
        else
        {
            if (_spawnedBullets.Count < 1)
                return;

            foreach (Bullet bullet in _spawnedBullets)
            {
                if (bullet.MoveCoroutine == null)
                {
                    bullet.MoveCoroutine = _coroutineRunner.StartCoroutine(MoveForward(bullet));
                }
            }
        }
    }
}
