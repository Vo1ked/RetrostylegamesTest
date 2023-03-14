using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour, IBulletSpawn, IDamageble, IPauseHandler
{
	public event Action<Vector3> PlayerStartTeleport = (Vector3) => { };

	[SerializeField] private Transform _camera;
	[SerializeField] private Transform _bulletSpawnPoint;
	[SerializeField] private float _moveThreshold = 0.01f;
	[SerializeField] private float _rotateThreshold = 0.01f;

	private Rigidbody _rigidbody;

	private Vector3 _direction;
	private Vector3 _rotation;
	private float _angleHorizontal;
	private float _angleVertical;
	private Quaternion _originRotation;
	private Quaternion _originCameraRotation;

	private float _attackReloadTimeLeft;
	private bool _moving;
	private bool _rotating;

	private PlayerStats _playerStats;
	private IPlayerInput _input;
	private SpawnPositionFactory _spawnPoisition;
	private PauseManager _pauseManager;
	[Inject]
	private void Construct(IPlayerInput input, SpawnPositionFactory spawnPoisition, PauseManager pauseManager, PlayerStats playerStats)
	{
		_input = input;
		_spawnPoisition = spawnPoisition;
		_pauseManager = pauseManager;
		_playerStats = playerStats;
	}

	public void OnPlayerTeleport()
	{
		PlayerStartTeleport.Invoke(transform.position);
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
		if (!isPause)
		{
			Reload(_attackReloadTimeLeft, _pauseManager.PauseCancellationToken.Token);
			_moving = false;
			_rotating = false;
		}
	}

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
		_playerStats.Heals.CurrentHeals = _playerStats.Heals.StartHeals;
		_playerStats.Mana.CurrentMana = _playerStats.Mana.StartMana;
		_pauseManager.SubscribeHandler(this);
	}

	private void Start()
	{

		_input.Direction += OnMoveDirectionChanged;
		_input.Rotation += OnMoveRotateChanged;
		_input.Fire += Attack;
		_originRotation = transform.localRotation;
		_originCameraRotation = _camera.localRotation;
		transform.position = _spawnPoisition.GetSpawnPosition(SpawnType.random);

		var container = FindObjectOfType<SceneContext>().Container;
		foreach (Ability ability in _playerStats.Abilities)
		{
			container.Inject(ability);
		}
	}
	private void OnMoveDirectionChanged(Vector2 direction)
	{
		_direction = transform.rotation * new Vector3(direction.x, 0, direction.y);
		if (!_moving && !_pauseManager.IsPaused)
		{
			Move(_pauseManager.PauseCancellationToken.Token);
		}
	}

	private void OnMoveRotateChanged(Vector2 rotation)
	{
		_rotation = rotation;
		if (!_rotating && !_pauseManager.IsPaused)
		{
			Rotate(_pauseManager.PauseCancellationToken.Token);
		}
	}

	private async void Move(CancellationToken token)
	{
		try
		{
			_moving = true;
			if (token.IsCancellationRequested)
			{
				_moving = false;
				return;
			}
			Vector3 movement = new Vector3(_direction.x, 0.0f, _direction.z);
			_rigidbody.MovePosition(transform.position + movement * _playerStats.MoveSpeed * Time.deltaTime);
			await Task.Delay(Mathf.RoundToInt(Time.fixedDeltaTime * 1000f), token);
			if (_direction.magnitude > _moveThreshold && !token.IsCancellationRequested)
			{
				Move(token);
			}
			_moving = false;
		}
		catch (TaskCanceledException) { }
	}

	private async void Rotate(CancellationToken token)
	{
		try
		{
			_rotating = true;
			if (token.IsCancellationRequested)
			{
				_rotating = false;
				return;
			}
			_angleHorizontal += _rotation.x * _playerStats.RotationSpeed;
			_angleVertical -= _rotation.y * _playerStats.RotationSpeed;

			_angleVertical = Mathf.Clamp(_angleVertical, _playerStats.MaxUpRotation, _playerStats.MaxDownRotation);

			Quaternion rotationY = Quaternion.AngleAxis(_angleHorizontal, Vector3.up);
			Quaternion rotationX = Quaternion.AngleAxis(_angleVertical, Vector3.right);

			transform.localRotation = _originRotation * rotationY;
			_camera.localRotation = _originCameraRotation * rotationX;
			await Task.Delay(Mathf.RoundToInt(Time.fixedDeltaTime * 1000f), token);
			if (_rotation.magnitude > _rotateThreshold && !token.IsCancellationRequested)
			{
				Rotate(token);
			}
			_rotating = false;
		}
		catch (TaskCanceledException) { }
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
			Reload(attackAbility, _pauseManager.PauseCancellationToken.Token);
			abilities.Remove(overrideAbility);
		}

		foreach (Ability ability in abilities)
		{
			ability.Execute(gameObject, this);
		}

	}

	private async void Reload(float timer, CancellationToken token)
	{
		_attackReloadTimeLeft = timer;
		var stopwatch = new System.Diagnostics.Stopwatch();
		try
		{
			stopwatch.Start();
			await Task.Delay(Mathf.RoundToInt(timer * 1000f), token);
			if (token.IsCancellationRequested)
				return;
			_attackReloadTimeLeft = 0;
		}
		catch (TaskCanceledException)
		{
			_attackReloadTimeLeft = timer - (float)stopwatch.Elapsed.TotalSeconds;
		}

		stopwatch.Stop();
	}

	private void OnDestroy()
	{
		_input.Direction -= OnMoveDirectionChanged;
		_input.Rotation -= OnMoveRotateChanged;
		_input.Fire -= Attack;
		_pauseManager.UnsubscribeHandler(this);
	}
}
