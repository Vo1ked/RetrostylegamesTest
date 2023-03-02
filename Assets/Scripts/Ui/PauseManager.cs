using System.Collections.Generic;
public class PauseManager
{
    public bool IsPaused { get; private set; } = false;

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
        _pauseHandlers.ForEach(x => x.OnPause(isPaused));
    }
}
