using UnityEngine;

internal interface IMovable
{
    Rigidbody Rigidbody { get; }
    Vector3 TargetPosition { get; set; }
    System.Action CompletedMove { get; set; }
}