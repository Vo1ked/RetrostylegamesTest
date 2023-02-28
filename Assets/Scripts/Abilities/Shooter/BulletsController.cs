using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Zenject;
using System;

[Serializable]
public class BulletsController : IPauseHandler
{
    [HideInInspector] public GameObject Shooter;
    [SerializeField] private ITargetSearch _target;
    [SerializeField] private Vector3 _spawnOffset;

    [SerializeField] private BulletsStats _bulletsStats;
    [SerializeField] protected List<Bullet> _spawnedBullets;

    protected BulletContainer _bulletContainer;
    protected CoroutineRunner _coroutineRunner;
    protected static float _autoDeleteTimer = 60;
    protected PauseManager _pauseManager;

    [Inject]
    private void Construct(BulletContainer bulletContainer, CoroutineRunner coroutineRunner, PauseManager pauseManager)
    {
        Debug.Log("BulletsController Construct start");
        _bulletContainer = bulletContainer;
        _coroutineRunner = coroutineRunner;
        _pauseManager = pauseManager;
        _pauseManager.SubscribeHandler(this);
        _spawnedBullets = new List<Bullet>();
        Debug.Log("BulletsController Construct end");

    }



    public virtual Bullet Spawn()
    {
        if (_bulletsStats.Bullet == null)
        {
            Debug.LogError($"Bullet {_bulletsStats.name} did`t contain  bullet");
            return null;
        }
        var spawnPosition = Shooter.GetComponent<IBulletSpawn>().GetSpawnPosition();
        var _spawnedBullet = GameObject.Instantiate<Bullet>(_bulletsStats.Bullet, spawnPosition + _spawnOffset, Quaternion.identity, _bulletContainer.transform);
        _spawnedBullet.name = $"{_bulletsStats.name}_{Shooter.name}";
        _spawnedBullet.Hited += OnCollision;
        _spawnedBullet.TimeToDeleteLeft = _autoDeleteTimer;
        _spawnedBullets.Add(_spawnedBullet);
        return _spawnedBullet;

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
        SetMoveDirection(bullet);
        bullet.MoveCoroutine = _coroutineRunner.StartCoroutine(MoveForward(bullet));
    }

    private void SetMoveDirection(Bullet bullet)
    {
        var spawnPosition = Shooter.GetComponent<IBulletSpawn>().GetSpawnPosition();
        Vector3 direction = (spawnPosition - Shooter.transform.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.LookAt(Shooter.transform);

    }

    protected virtual void Hit(Bullet bullet,IDamageble damageble)
    {
        damageble.Damage(_bulletsStats.HitInfo);
        Destroy(bullet);
    }

    protected virtual void Destroy(Bullet bullet)
    {
        bullet.Hited -= OnCollision;
        _spawnedBullets.Remove(bullet);
    }

    private IEnumerator MoveForward(Bullet bullet)
    {
        var rigidBody = bullet.GetComponent<Rigidbody>();
        while (bullet.TimeToDeleteLeft > 0)
        {
            rigidBody.MovePosition(bullet.transform.position + -bullet.transform.forward * _bulletsStats.Speed * Time.deltaTime);
            yield return null;
            bullet.TimeToDeleteLeft -= Time.deltaTime;
        }
        Debug.Log("MoveForward end");

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
                _coroutineRunner.StopRunningCoroutine(bullet.MoveCoroutine);
            }
        }
        else
        {
            if (_spawnedBullets.Count < 1)
                return;

            foreach (Bullet bullet in _spawnedBullets)
            {
                bullet.MoveCoroutine = _coroutineRunner.StartCoroutine(MoveForward(bullet));
            }
        }
    }
}
