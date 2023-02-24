using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour {


	private Rigidbody _rigidbody;
	private IPlayerInput _input;
	private PlayerStats _playerStats;

	private Vector3 _direction;
	private Vector3 _rotation;
	private Coroutine _moveCoroutine;
	private Coroutine _rotateCoroutine;


	[Inject]
	private void Construct(IPlayerInput input,PlayerStats playerStats)
    {
		_input = input;
		_playerStats = playerStats;
    }

	private void Awake()
    {
		_rigidbody = GetComponent<Rigidbody>();
    }
	// Use this for initialization
	void Start () {

		_input.Direction += OnMoveDirectionChanged;
		_input.Rotation += OnMoveRotateChanged;

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
			Debug.Log(currentSpeed);
			yield return fixedUpdate;
		}

		_moveCoroutine = null;
    }

	private IEnumerator Rotate()
	{

		while (_rotation != Vector3.zero)
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_rotation),
				_playerStats.RotationSpeed * Time.deltaTime);
			yield return null;
		}

		_rotateCoroutine = null;
	}

	private void OnMoveRotateChanged(Vector2 rotation)
	{
		_rotation = new Vector3(rotation.x, 0f, rotation.y);
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
