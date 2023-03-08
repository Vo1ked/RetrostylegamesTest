using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "FlyToPlayer", menuName = "My Game/Ability/FlyToPlayer")]
public class FlyToPlayer : Ability, IPauseHandler
{
    public override Specialization Specialization => Specialization.Move;
    public override WorkType WorkType => WorkType.@override;

    [SerializeField] private float _flySpeed;

    private List<IMovable> _movables = new List<IMovable>();

    private Player _player;
    private PauseManager _pauseManager;
    [Inject]
    private void Construct( PauseManager pauseManager, Player player)
    {
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
        Fly(movable, _pauseManager.PauseCancellationToken.Token);
        _movables.Add(movable);
    }

    private async void Fly(IMovable movable, CancellationToken token)
    {
        try
        {
            if (token.IsCancellationRequested)
            {
                return;
            }
            var direction = (_player.transform.position - movable.Rigidbody.position).normalized;
            movable.Rigidbody.MovePosition(movable.Rigidbody.position + direction * _flySpeed * Time.deltaTime);

            await Task.Delay(Mathf.RoundToInt(Time.fixedDeltaTime * 1000f), token);

            if (token.IsCancellationRequested)
            {
                return;
            }
            Fly(movable, token);
        }
        catch (TaskCanceledException) { }
    }

    public void OnPause(bool IsPause)
    {
        if (_movables.Count < 1)
            return;

        if (!IsPause)
        {
            foreach (IMovable movable in _movables)
            {
                Fly(movable, _pauseManager.PauseCancellationToken.Token);
            }
        }
    }
}
