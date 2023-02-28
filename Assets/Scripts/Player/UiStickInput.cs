using System;
using UnityEngine;
using UnityEngine.UI;

public class UiStickInput : MonoBehaviour, IPlayerInput
{
	[SerializeField] private UiStick _moveStick;
	[SerializeField] private UiStick _rotateStick;
	[SerializeField] private Button _fireButton;

    public event Action<Vector2> Direction = (Vector2) => { };
    public event Action<Vector2> Rotation = (Vector2) => { };
    public event Action Fire = () => { };

    void Start () {
        _moveStick.OnPositionChanged += OnChangeDirection;
        _rotateStick.OnPositionChanged += OnChangeRotation;
        _fireButton.onClick.AddListener(OnFireClick);
    }

    private void OnFireClick()
    {
        Fire.Invoke();
    }

    private void OnChangeDirection(Vector2 direction)
    {
        Direction.Invoke(direction);
    }

    private void OnChangeRotation(Vector2 rotation)
    {
        Rotation.Invoke(rotation);
    }

    public void OnDestroy()
    {
        _moveStick.OnPositionChanged -= OnChangeDirection;
        _rotateStick.OnPositionChanged -= OnChangeRotation;
        _fireButton.onClick.RemoveAllListeners();

    }
}
