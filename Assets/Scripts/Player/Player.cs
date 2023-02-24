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
	private Vector2 _rotation;
	private Coroutine _moveCoroutine;


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
		Debug.Log("World direction =" + _direction);
		_direction = transform.rotation * new Vector3(direction.x, 0, direction.y);
		Debug.Log("loca; direction ="+ _direction);


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
			float currentSpeed = _rigidbody.velocity.magnitude;
			if (currentSpeed > _playerStats.MaxMoveSpped)
			{
				float brakeSpeed = currentSpeed - _playerStats.MaxMoveSpped;
				Vector3 normalisedVelocity = _rigidbody.velocity.normalized;
				Vector3 brakeVelocity = normalisedVelocity * brakeSpeed;
				_rigidbody.AddForce(-brakeVelocity);
			}
			yield return fixedUpdate;
		}

		_moveCoroutine = null;
    }

	private void OnMoveRotateChanged(Vector2 rotation)
	{

	}
	// Update is called once per frame
	void Update () {
		
	}

	private void OnDestroy()
    {
		_input.Direction -= OnMoveDirectionChanged;
		_input.Rotation -= OnMoveRotateChanged;
	}
}
