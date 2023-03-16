using RetroStyleGamesTest.Zenject;
using RetroStyleGamesTest.Pause;
using RetroStyleGamesTest.Units;

using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace RetroStyleGamesTest.Abillity.Implementation
{
	[CreateAssetMenu(fileName = "FlyAtSpawn", menuName = "My Game/Ability/FlyAtSpawn")]
	public class FlyAtSpawn : Ability, IPauseHandler, System.IDisposable
	{
		public override Specialization Specialization => Specialization.Spawn;
		public override WorkType WorkType => WorkType.@override;

		[SerializeField] private float _flyUpDistance;
		[SerializeField] private float _flyUpSpeed;
		[SerializeField] private float _afterFlyUpBeforeMoveDelay;

		[System.NonSerialized] private DisposeOnSceneExit _onSceneExit;
		[System.NonSerialized] private List<IMovable> _movables = new List<IMovable>();
		[System.NonSerialized] private PauseManager _pauseManager;
		[Inject]
		private void Construct(PauseManager pauseManager, DisposeOnSceneExit onSceneExit)
		{
			_pauseManager = pauseManager;
			_pauseManager.SubscribeHandler(this);
			_onSceneExit = onSceneExit;
			_onSceneExit.Add(this);
		}

		public void Dispose()
		{
			_movables.Clear();
			_pauseManager.UnsubscribeHandler(this);
		}

		public void OnPause(bool IsPause)
		{
			if (_movables.Count < 1)
				return;

			if (!IsPause)
			{
				foreach (IMovable movable in _movables)
				{
					FlyUp(movable, _pauseManager.PauseCancellationToken.Token);
				}
			}
		}

		public override void Execute(GameObject user, params object[] parameters)
		{
			var movable = user.GetComponent<IMovable>();
			movable.TargetPosition = user.transform.position + Vector3.up * _flyUpDistance;

			FlyUp(movable, _pauseManager.PauseCancellationToken.Token);
			_movables.Add(movable);
		}

		private async void FlyUp(IMovable movable, CancellationToken token)
		{
			try
			{
				while (movable.Rigidbody != null &&
					Vector3.Distance(movable.Rigidbody.transform.position, movable.TargetPosition) > 0.1f)
				{
					if (token.IsCancellationRequested)
						return;

					var direction = (movable.TargetPosition - movable.Rigidbody.transform.position).normalized;
					movable.Rigidbody.MovePosition(
						movable.Rigidbody.transform.position + direction * _flyUpSpeed);
					await Task.Delay(Mathf.RoundToInt(Time.fixedDeltaTime * 1000f), token);

				}
				await Task.Delay(Mathf.RoundToInt(_afterFlyUpBeforeMoveDelay * 1000f), token);
				if (token.IsCancellationRequested)
					return;

				_movables.Remove(movable);
				movable.CompletedMove?.Invoke();
			}
			catch (TaskCanceledException) { }
		}
	}
}
