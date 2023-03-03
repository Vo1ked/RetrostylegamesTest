using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "FlyToPlayer", menuName = "My Game/Ability/FlyToPlayer")]
public class FlyToPlayer : Ability, IPauseHandler {
    public override Specialization Specialization => Specialization.Move;
    public override WorkType WorkType => WorkType.@override;

	[SerializeField] private float _flyUpSpeed;

	private Player _player;
	private List<IMovable> _movables = new List<IMovable>();
	private CoroutineRunner _coroutineRunner;
	private PauseManager _pauseManager;
	[Inject]
	private void Construct(CoroutineRunner coroutineRunner, PauseManager pauseManager, Player player)
	{
		_coroutineRunner = coroutineRunner;
		_pauseManager = pauseManager;
        _player = player;
		_pauseManager.SubscribeHandler(this);

		_player.OnPlayerDestroy += OnPlayerDestroy;
	}

	private void OnPlayerDestroy()
	{
		_player.OnPlayerDestroy -= OnPlayerDestroy;
		_movables.Clear();
		_pauseManager.UnsubscribeHandler(this);
	}

	public override void Execute(GameObject user, params object[] parameters)
	{
		var movable = user.GetComponent<IMovable>();
		movable.Coroutine = _coroutineRunner.RunCoroutine(Fly(movable));
		_movables.Add(movable);
	}

	private IEnumerator Fly(IMovable movable)
	{
		if (_pauseManager.IsPaused)
		{
			yield break;
		}
		var direction = (_player.transform.position - movable.Rigidbody.transform.position).normalized;
		movable.Rigidbody.MovePosition(movable.Rigidbody.transform.position + direction * _flyUpSpeed * Time.deltaTime);

		yield return new WaitForFixedUpdate();

		if (movable.Coroutine != null)
		{
			_coroutineRunner.StopRunningCoroutine((movable.Coroutine));
			movable.Coroutine = null;
		}
		movable.Coroutine = _coroutineRunner.RunCoroutine(Fly(movable));
	}

	public void OnPause(bool IsPause)
	{
		if (_movables.Count < 1)
			return;

		if (IsPause)
		{
			foreach (IMovable movable in _movables)
			{
				if (movable.Coroutine != null)
				{
					_coroutineRunner.StopRunningCoroutine(movable.Coroutine);
					movable.Coroutine = null;
				}
			}
		}
		else
		{
			foreach (IMovable movable in _movables)
			{
				if (movable.Coroutine == null)
				{
					movable.Coroutine = _coroutineRunner.RunCoroutine(Fly(movable));
				}
			}
		}
	}
}
