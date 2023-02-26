using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour {

	public event Action<Vector3> PlayerStartTeleport = (Vector3) => { };

	[SerializeField] private Transform _camera;

	private Rigidbody _rigidbody;

	private Vector3 _direction;
	private Vector3 _rotation;
	private Coroutine _moveCoroutine;
	private Coroutine _rotateCoroutine;

	private float _angleHorizontal;
	private float _angleVertical;
	private Quaternion _originRotation;
	private Quaternion _originCameraRotation;

	private IPlayerInput _input;
	private PlayerStats _playerStats;
	private ISpawnPoisition _spawnPoisition;
	[Inject]
	private void Construct(IPlayerInput input,PlayerStats playerStats,ISpawnPoisition spawnPoisition)
    {
		_input = input;
		_playerStats = playerStats;
		_spawnPoisition = spawnPoisition;
    }

	public void OnPlayerTeleport()
    {
		PlayerStartTeleport.Invoke(transform.position);
    }

	private void Awake()
    {
		_rigidbody = GetComponent<Rigidbody>();
    }

	private void Start () {

		_input.Direction += OnMoveDirectionChanged;
		_input.Rotation += OnMoveRotateChanged;
		_originRotation = transform.localRotation;
		_originCameraRotation = _camera.localRotation;
		transform.position = _spawnPoisition.GetSpawnPosition();
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

		while(_direction != Vector3.zero)
        {
			Vector3 movement = new Vector3(_direction.x, 0.0f, _direction.z);
			_rigidbody.AddForce(movement * _playerStats.MoveSpeed);
			var speed = Mathf.Clamp(_rigidbody.velocity.magnitude, 0, _playerStats.MaxMoveSpped);
			_rigidbody.velocity = _rigidbody.velocity.normalized * speed;
			float currentSpeed = _rigidbody.velocity.magnitude;
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
	}
}
