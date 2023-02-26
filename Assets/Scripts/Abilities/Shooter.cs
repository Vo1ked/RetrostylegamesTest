using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "Shooter", menuName = "My Game/Ability/Shooter")]
public class Shooter : Ability, IPauseHandler
{
    [Space]
    [SerializeField] private ABullet Bullet;
    [SerializeField] private float ReloadTime;
    private float _reloadTime;
    private Coroutine _reload;
    private CoroutineRunner _coroutineRunner;
    private PauseManager _pauseManager;

    [Inject]
    private void Construct(CoroutineRunner coroutineRunner, PauseManager pauseManager)
    {
        _coroutineRunner = coroutineRunner;
        _pauseManager = pauseManager;

        _pauseManager.SubscribeHandler(this);

        Specialization = Specialization.Attack;
        WorkType = WorkType.@override;
    }

    public override void Execute(GameObject user, params object[] parameters)
    {
        if (_reload != null || _pauseManager.IsPaused)
            return;

        Bullet.Shooter = user;
        Bullet.Spawn();
        Bullet.Move();
        _reload = _coroutineRunner.RunCoroutine(Reload(ReloadTime));
    }

    private IEnumerator Reload(float timer)
    {
        float _reloadTime = timer;

        while (_reloadTime > 0)
        {
            yield return null;
            _reloadTime -= Time.deltaTime;
        }

        _reload = null;
    }

    public void OnPause(bool isPause)
    {
        if (isPause)
        {
            _coroutineRunner.StopCoroutine(_reload);
            _reload = null;
        }
        else
        {
            _reload = _coroutineRunner.RunCoroutine(Reload(_reloadTime));
        }
    }
}
