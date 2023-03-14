using UnityEngine;
using System.Threading;

public class Bullet : MonoBehaviour
{
	public event System.Action<Bullet, Collider> Hited;
	public CancellationTokenSource DestroyCancellationToken;
	public float TimeToDeleteLeft;
	[HideInInspector] public GameObject Shooter;

	[SerializeField] private CollisionEventRepeater _collisionEventRepeater;

	public void OnEnable()
	{
		_collisionEventRepeater.TriggerEnter += OnHit;
	}

	public void OnDisable()
	{
		_collisionEventRepeater.TriggerEnter -= OnHit;
	}

	private void OnHit(Collider collider)
	{
		Hited.Invoke(this, collider);
	}
}
