using RetroStyleGamesTest.Zenject;
using RetroStyleGamesTest.Pause;
using RetroStyleGamesTest.Units;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace RetroStyleGamesTest.Abillity.Implementation
{
    [CreateAssetMenu(fileName = "FlyToPlayer", menuName = "My Game/Ability/FlyToPlayer")]
    public class FlyToPlayer : Ability, IPauseHandler, System.IDisposable
    {
        public override Specialization Specialization => Specialization.Move;
        public override WorkType WorkType => WorkType.@override;

        [SerializeField] private float _flySpeed;

        [System.NonSerialized] private List<IMovable> _movables = new List<IMovable>();

        [System.NonSerialized] private Player _player;
        [System.NonSerialized] private DisposeOnSceneExit _onSceneExit;
        [System.NonSerialized] private PauseManager _pauseManager;
        [Inject]
        private void Construct(PauseManager pauseManager, Player player, DisposeOnSceneExit onSceneExit)
        {
            _pauseManager = pauseManager;
            _player = player;
            _pauseManager.SubscribeHandler(this);
            _onSceneExit = onSceneExit;
            _onSceneExit.Add(this);
        }

        public void Dispose()
        {
            _movables.Clear();
            _pauseManager.UnsubscribeHandler(this);
        }

        public override void Execute(GameObject user, params object[] parameters)
        {
            var movable = user.GetComponent<IMovable>();
            var destroyable = user.GetComponent<IDestroyable>();
            destroyable.DestroyCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_pauseManager.PauseCancellationToken.Token);
            Fly(movable, destroyable.DestroyCancellationToken.Token);
            _movables.Add(movable);
        }

        public void OnPause(bool IsPause)
        {
            if (_movables.Count < 1)
                return;

            if (!IsPause)
            {
                foreach (IMovable movable in _movables)
                {
                    if (movable.Rigidbody == null)
                        continue;
                    var destroyable = movable.Rigidbody.GetComponent<IDestroyable>();
                    destroyable.DestroyCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_pauseManager.PauseCancellationToken.Token);
                    Fly(movable, destroyable.DestroyCancellationToken.Token);
                }
            }
        }

        private async void Fly(IMovable movable, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested || movable.Rigidbody == null)
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
    }
}