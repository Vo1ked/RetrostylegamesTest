using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour, IBulletSpawn, IDamageble
{
	public event Action<Vector3> PlayerStartTeleport = (Vector3) => { };

	[SerializeField] private Transform _camera;
	[SerializeField] private Transform _bulletSpawnPoint;
	[SerializeField] private PlayerStats _playerStats;

	private Rigidbody _rigidbody;

	private Vector3 _direction;
	private Vector3 _rotation;
	private float _angleHorizontal;
	private float _angleVertical;
	private Quaternion _originRotation;
	private Quaternion _originCameraRotation;

	private float _attackReloadTimeLeft;

	private Coroutine _moveCoroutine;
	private Coroutine _rotateCoroutine;
	private Coroutine _reloadCoroutine;

	private IPlayerInput _input;
	private ISpawnPoisition _spawnPoisition;
	[Inject]
	private void Construct(IPlayerInput input, ISpawnPoisition spawnPoisition)
	{
		_input = input;
		_spawnPoisition = spawnPoisition;
	}

	public void OnPlayerTeleport()
	{
		PlayerStartTeleport.Invoke(transform.position);
	}

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
		_playerStats.Heals.CurrentHeals = _playerStats.Heals.StartHeals;
		_playerStats.Mana.CurrentMana = _playerStats.Mana.StartMana;
	}

	private void Start()
	{

		_input.Direction += OnMoveDirectionChanged;
		_input.Rotation += OnMoveRotateChanged;
		_input.Fire += Attack;
		_originRotation = transform.localRotation;
		_originCameraRotation = _camera.localRotation;
		transform.position = _spawnPoisition.GetSpawnPosition();

		var container = FindObjectOfType<SceneContext>().Container;
		foreach (Ability ability in _playerStats.Abilities)
		{
			container.Inject(ability);
		}
	}


	private void OnMoveDirectionChanged(Vector2 direction)
	{
		_direction = transform.rotation * new Vector3(direction.x, 0, direction.y);

		if (_moveCoroutine == null)
		{
			_moveCoroutine = StartCoroutine(Move());
		}
	}

	private IEnumerator Move()
	{
		var fixedUpdate = new WaitForFixedUpdate();

		while (_direction != Vector3.zero)
		{
			Vector3 movement = new Vector3(_direction.x, 0.0f, _direction.z);
			_rigidbody.MovePosition(transform.position + movement * _playerStats.MoveSpeed * Time.deltaTime);
            var speed = Mathf.Clamp(_rigidbody.velocity.magnitude, 0, _playerStats.MaxMoveSpped);
            _rigidbody.velocity = _rigidbody.velocity.normalized * speed;
            yield return fixedUpdate;
		}

		_moveCoroutine = null;
	}

	private IEnumerator Rotate()
	{
		while (_rotation != Vector3.zero)
		{
			_angleHorizontal += _rotation.x * _playerStats.RotationSpeed;
			_angleVertical -= _rotation.y * _playerStats.RotationSpeed;


			_angleVertical = Mathf.Clamp(_angleVertical, _playerStats.MaxUpRotation, _playerStats.MaxDownRotation);

			Quaternion rotationY = Quaternion.AngleAxis(_angleHorizontal, Vector3.up);
			Quaternion rotationX = Quaternion.AngleAxis(_angleVertical, Vector3.right);

			transform.localRotation = _originRotation * rotationY;
			_camera.localRotation = _originCameraRotation * rotationX;
			yield return null;
		}

		_rotateCoroutine = null;
	}

	private void Attack()
	{
		if (_attackReloadTimeLeft > 0)
		{
			return;
		}

		List<Ability> abilities = new List<Ability>();
		if (!Ability.AbilityCheck(_playerStats.Abilities, Specialization.Attack, ref abilities))
		{
			return;
		}
		var overrideAbility = abilities.First(abbility => abbility.WorkType == WorkType.@override);

		if (overrideAbility != null)
		{
			var attackAbility = ((IAttackAbillity)overrideAbility).ReloadTime;
			overrideAbility.Execute(gameObject, this);
			_reloadCoroutine = StartCoroutine(Reload(attackAbility));
			abilities.Remove(overrideAbility);
		}

		foreach (Ability ability in abilities)
		{
			ability.Execute(gameObject, this);
		}

	}

	private IEnumerator Reload(float timer)
	{
		_attackReloadTimeLeft = timer;

		while (_attackReloadTimeLeft > 0)
		{
			yield return null;
			_attackReloadTimeLeft -= Time.deltaTime;
		}
		_reloadCoroutine = null;
	}

	private void OnMoveRotateChanged(Vector2 rotation)
	{
		_rotation = rotation;
		if (_rotateCoroutine == null)
		{
			_rotateCoroutine = StartCoroutine(Rotate());
		}


	}

	private void OnDestroy()
	{
		_input.Direction -= OnMoveDirectionChanged;
		_input.Rotation -= OnMoveRotateChanged;
		_input.Fire -= Attack;
	}

	public Vector3 GetSpawnPosition()
	{
		return _bulletSpawnPoint.position;
	}

    public void Damage(HitInfo hit)
    {
		_playerStats.Heals.CurrentHeals -= hit.HealsDamage;
		_playerStats.Mana.CurrentMana -= hit.ManaDamage;
	}

	public void OnPause(bool isPause)
	{
		if (isPause)
		{
			if (_reloadCoroutine == null)
				return;

			StopCoroutine(_reloadCoroutine);
			_reloadCoroutine = null;
		}
		else
		{
			_reloadCoroutine = StartCoroutine(Reload(_attackReloadTimeLeft));
		}
	}
}
