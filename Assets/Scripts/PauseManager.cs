using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager {

	public bool IsPaused { get; private set; }

	private List<IPauseHandler> _pauseHandlers = new List<IPauseHandler>();


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
        foreach (IPauseHandler handler  in _pauseHandlers)
        {
            handler.OnPause(IsPaused);
        }
    }

}
