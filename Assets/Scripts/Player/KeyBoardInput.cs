using System;
using UnityEngine;
using Zenject;

public class KeyBoardInput : MonoBehaviour, IPlayerInput {
    public event Action<Vector2> Direction = (Vector2) => { };
    public event Action<Vector2> Rotation = (Vector2) => { };
    private PauseManager _pauseManager;

    [Inject]
    private void Construct(PauseManager pauseManager)
    {
        _pauseManager = pauseManager;
    }
    public void OnPause(bool IsPause)
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Cancel"))
        {
            _pauseManager.SetPause(!_pauseManager.IsPaused);
        }

        if (_pauseManager.IsPaused)
            return;

        Vector2 moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Direction.Invoke(moveDirection);

        Vector2 rotation = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Rotation.Invoke(rotation);

    }
}
