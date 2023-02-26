using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Zenject;

[CreateAssetMenu(fileName = "Shooter", menuName = "My Game/Bullet/Shooter")]
public abstract class ABullet : ScriptableObject , IPauseHandler {

    public GameObject Shooter;
    [SerializeField] private ITargetSearch _target;
    [SerializeField] private Vector3 _spawnOffset;

    [SerializeField] private BulletsStats _bulletsStats;
    private GameObject _body;
    protected Coroutine _moveCorutine;

    protected BulletContainer _bulletContainer;
    protected CoroutineRunner _coroutineRunner;
    protected static float _autoDeleteTimer = 60;
    protected float _timeToDeleteLeft = _autoDeleteTimer;
    protected PauseManager _pauseManager;
    [Inject]
    private void Construct(BulletContainer bulletContainer, CoroutineRunner coroutineRunner, PauseManager pauseManager)
    {
        _bulletContainer = bulletContainer;
        _coroutineRunner = coroutineRunner;
        _pauseManager = pauseManager;
        _pauseManager.SubscribeHandler(this);
    }

    private bool AbilityCheck(Specialization specialization, ref List<Ability> abilities)
    {
        if (_bulletsStats.Abilities.Count < 1)
            return false;

        abilities = _bulletsStats.Abilities.FindAll(abbility => abbility.Specialization == specialization);
        var overrideAbilities = abilities.FindAll(abbility => abbility.WorkType == WorkType.@override);
        if (overrideAbilities.Count > 1)
        {
            Debug.LogError($"override abiliries in {_bulletsStats.name} with specialization {overrideAbilities[0].Specialization} more that one! will work only one");
        }
        return abilities.Count > 0;
    }

	public virtual void Spawn()
    {
        List<Ability> abilities = new List<Ability>();
        if (!AbilityCheck(Specialization.Spawn,ref abilities))
        {
            _body = GameObject.Instantiate<GameObject>(_bulletsStats.Body, Shooter.transform.position + _spawnOffset, Shooter.transform.rotation, _bulletContainer.transform);
            _body.name = $"{_bulletsStats.name}_{Shooter.name}";
        }
        var overrideAbilities = abilities.FindAll(abbility => abbility.WorkType == WorkType.@override);

    }

    public virtual void Move()
    {
        _moveCorutine = _coroutineRunner.StartCoroutine(MoveForward());
    }

    protected virtual void Hit()
    {

    }

    private IEnumerator MoveForward()
    {
        while(_timeToDeleteLeft > 0)
        {
            _body.transform.Translate(_body.transform.forward * _bulletsStats.Speed * Time.deltaTime);
            yield return null;
            _timeToDeleteLeft -= Time.deltaTime;
        }
        _moveCorutine = null;
    }

    public void OnPause(bool IsPause)
    {
        if (IsPause)
        {
            _coroutineRunner.StopRunningCoroutine(_moveCorutine);
        }
        else
        {
            if (_timeToDeleteLeft > 0)
            {
                _moveCorutine = _coroutineRunner.StartCoroutine(MoveForward());
            }
        }
    }
    /*   
   private void Hit();
   private void Destroy();*/


}
