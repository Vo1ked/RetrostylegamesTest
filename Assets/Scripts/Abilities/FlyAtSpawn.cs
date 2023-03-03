using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "FlyAtSpawn", menuName = "My Game/Ability/FlyAtSpawn")]
public class FlyAtSpawn : Ability, IPauseHandler
{
	public override Specialization Specialization => Specialization.Spawn;
	public override WorkType WorkType => WorkType.@override;

	[SerializeField] private float _flyUpDistance;
	[SerializeField] private float _flyUpSpeed;
	[SerializeField] private float _afterFlyUpBeforeMoveDelay;

	private List<IMovable> _movables = new List<IMovable>();
	private CoroutineRunner _coroutineRunner;
	private PauseManager _pauseManager;
	[Inject]
	private void Construct(CoroutineRunner coroutineRunner, PauseManager pauseManager, Dispose dispose)
	{
		_coroutineRunner = coroutineRunner;
		_pauseManager = pauseManager;
		_pauseManager.SubscribeHandler(this);

		dispose.OnDispose += () => _pauseManager.UnsubscribeHandler(this);
	}
	public override void Execute(GameObject user, params object[] parameters)
    {
		var movable = user.GetComponent<IMovable>();
		movable.TargetPosition = user.transform.position + Vector3.up * _flyUpDistance;

		movable.Coroutine = _coroutineRunner.RunCoroutine(FlyUp(movable));
		_movables.Add(movable);
	}

    private IEnumerator FlyUp(IMovable movable)
    {
        while (Vector3.Distance (movable.Rigidbody.transform.position, movable.TargetPosition) > 0.1f)
        {
			if (_pauseManager.IsPaused)
			{
				yield break;
			}
			var direction = (movable.TargetPosition - movable.Rigidbody.transform.position).normalized;
			movable.Rigidbody.MovePosition(movable.Rigidbody.transform.position + direction * _flyUpSpeed * Time.deltaTime);
			yield return null;
		}

		var wait = new WaitForSeconds(_afterFlyUpBeforeMoveDelay);
		yield return wait;
        if (movable != null && !_pauseManager.IsPaused)
        {
			_movables.Remove(movable);
			movable.CompletedMove?.Invoke();
			movable.Coroutine = null;
		}
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
					movable.Coroutine = _coroutineRunner.RunCoroutine(FlyUp(movable));
				}
			}
		}
	}
}
