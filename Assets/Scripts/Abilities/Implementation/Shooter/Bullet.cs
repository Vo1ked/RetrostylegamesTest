using UnityEngine;
using System.Threading;
using RetroStyleGamesTest.Utils;
using RetroStyleGamesTest.Units;

namespace RetroStyleGamesTest.Abillity.Implementation
{
	public class Bullet : MonoBehaviour , IDestroyable
	{
		public event System.Action<Bullet, Collider> Hited;
		public float TimeToDeleteLeft;
		public CancellationTokenSource DestroyCancellationToken { get; set; }

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
}
