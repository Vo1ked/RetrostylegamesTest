using RetroStyleGamesTest.Input;
using RetroStyleGamesTest.Pause;

using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : IPlayerInput,IDisposable,IPauseHandler
{
    public event Action<Vector2> Direction;
    public event Action<Vector2> Rotation;
    public event Action Fire;
    public event Action Ultimate;
    public event Action Pause;

    private bool _mouseLocked;

    private GameInput _input;
    private PauseManager _pauseManager;
    public PlayerInput(PauseManager pauseManager)
    {
        _input = new GameInput();
        _input.Player.Move.performed += PlayerMove;
        _input.Player.Move.canceled += ResetMove;

        _input.Player.Rotation.performed += PlayerRotation;
        _input.Player.Rotation.canceled += ResetRotation;

        _input.Player.Fire.performed += PlayerFire;
        _input.Player.Fire2.performed += PlayerFire2;

        _input.Player.Pause.performed += PauseClicked;
        _input.Player.LockMouse.performed += LockMouse;
        _input.Player.Enable();

        _pauseManager = pauseManager;
        _pauseManager.SubscribeHandler(this);
    }

    public void Dispose()
    {
        _pauseManager.UnsubscribeHandler(this);

        _input.Player.Move.performed -= PlayerMove;
        _input.Player.Move.canceled -= ResetMove;

        _input.Player.Rotation.performed -= PlayerRotation;
        _input.Player.Rotation.canceled -= ResetRotation;

        _input.Player.Fire.performed -= PlayerFire;
        _input.Player.Fire2.performed -= PlayerFire2;

        _input.Player.LockMouse.performed -= LockMouse;
    }

    public void OnPause(bool IsPause)
    {
        if (IsPause)
        {
            _input.Player.Disable();
            ResetMove(new InputAction.CallbackContext());
            ResetRotation(new InputAction.CallbackContext());
            LockMouse(new InputAction.CallbackContext());
        }
        else
        {
            _input.Player.Enable();
        }
    }
    private void PauseClicked(InputAction.CallbackContext pause)
    {
        Pause?.Invoke();
    }

    private void ResetRotation(InputAction.CallbackContext rotate)
    {
        Rotation?.Invoke(Vector2.zero);
    }

    private void ResetMove(InputAction.CallbackContext move)
    {
        Direction?.Invoke(Vector2.zero);
    }

    private void PlayerFire2(InputAction.CallbackContext fire2)
    {
        Ultimate?.Invoke();
    }

    private void PlayerFire(InputAction.CallbackContext fire)
    {
        Fire?.Invoke();
    }

    private void PlayerRotation(InputAction.CallbackContext rotateDirection)
    {
        Rotation?.Invoke(rotateDirection.ReadValue<Vector2>().normalized);
    }

    private void PlayerMove(InputAction.CallbackContext moveDirection)
    {
        Direction?.Invoke(moveDirection.ReadValue<Vector2>());
    }

    private void LockMouse(InputAction.CallbackContext context)
    {
        if (!_mouseLocked)
        {
            if (_pauseManager.IsPaused)
                return;
            _mouseLocked = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Mouse.current.WarpCursorPosition(new Vector2(Screen.width / 2, Screen.height / 2));
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _mouseLocked = false;
        }
    }

}
