using System;
using UnityEngine;
using Zenject;

public class KeyBoardInput : MonoBehaviour, IPlayerInput, IPauseHandler
{
    public event Action<Vector2> Direction = (Vector2) => { };
    public event Action<Vector2> Rotation = (Vector2) => { };
    public event Action Fire = () => { };
    public event Action Ultimate = () => { };

    private PauseManager _pauseManager;
    private UiManager _uiManager;
    [Inject]
    private void Construct(PauseManager pauseManager, UiManager uiManager)
    {
        _pauseManager = pauseManager;
        _uiManager = uiManager;

    }
    public void OnPause(bool IsPause)
    {
        if (IsPause)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            Direction.Invoke(Vector2.zero);
            Rotation.Invoke(Vector2.zero);
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        _pauseManager.SubscribeHandler(this);

    }

    private void Update()
    {
        if (Input.GetButton("Cancel"))
        {
            _uiManager.OnOptionsClick();
            return;
        }

        if (_pauseManager.IsPaused)
            return;

        Vector2 moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Direction.Invoke(moveDirection);

        Vector2 rotation = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Rotation.Invoke(rotation);

        if (Input.GetButtonDown("Fire1"))
        {
            Fire.Invoke();
        }
        if (Input.GetButtonDown("Fire2"))
        {
            Ultimate.Invoke();
        }
    }

    private void OnDestroy()
    {
        _pauseManager.UnsubscribeHandler(this);
    }
}
