﻿using UnityEngine;

public class Bullet : MonoBehaviour {

	[SerializeField] private CollisionEventRepeater _collisionEventRepeater;
	public event System.Action<Bullet, Collider> Hited;
	public Coroutine MoveCoroutine;
	public float TimeToDeleteLeft;

	public void OnEnable()
    {
		_collisionEventRepeater.TriggerEnter += OnHit;
    }

	private void OnHit(Collider collider)
    {
		Hited.Invoke(this, collider);
    }

	public void OnDisable()
    {
		_collisionEventRepeater.TriggerEnter -= OnHit;

	}
}
