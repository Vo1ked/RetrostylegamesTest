﻿using System;
using UnityEngine;

public class UiStickInput : MonoBehaviour,IPlayerInput {

	[SerializeField] private UiStick _moveStick;
	[SerializeField] private UiStick _rotateStick;

    public event Action<Vector2> Direction = (Vector2) => { };
    public event Action<Vector2> Rotation = (Vector2) => { };

    void Start () {
        _moveStick.OnPositionChanged += OnChangeDirection;
        _rotateStick.OnPositionChanged += OnChangeRotation;
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
    }
}
