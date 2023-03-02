using System;
using UnityEngine;

public interface IPlayerInput {

    event Action<Vector2> Direction;
    event Action<Vector2> Rotation;
    event Action Fire;
    event Action Ultimate;
}
