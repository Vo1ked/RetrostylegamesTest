﻿using System.Collections.Generic;
using System.Threading;

namespace RetroStyleGamesTest.Pause
{
    public class PauseManager : System.IDisposable
    {
        public CancellationTokenSource PauseCancellationToken;
        public bool IsPaused { get; private set; }

        private List<IPauseHandler> _pauseHandlers;

        public PauseManager()
        {
            PauseCancellationToken = new CancellationTokenSource();
            _pauseHandlers = new List<IPauseHandler>();
        }

        public void SubscribeHandler(IPauseHandler pauseHandler)
        {
            _pauseHandlers.Add(pauseHandler);
        }

        public void UnsubscribeHandler(IPauseHandler pauseHandler)
        {
            _pauseHandlers.Remove(pauseHandler);
        }

        public void SetPause(bool isPaused)
        {
            IsPaused = isPaused;
            if (isPaused)
            {
                PauseCancellationToken.Cancel();
                PauseCancellationToken.Dispose();
            }
            else
            {
                PauseCancellationToken = new CancellationTokenSource();
            }
            _pauseHandlers.ForEach(x => x.OnPause(isPaused));

        }

        public void Dispose()
        {
            if (!PauseCancellationToken.IsCancellationRequested)
            {
                PauseCancellationToken.Cancel();
            }
            PauseCancellationToken.Dispose();
            _pauseHandlers.Clear();
        }
    }
}
