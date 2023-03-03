using UnityEngine;

internal interface IMovable
{
    Coroutine Coroutine{get;set;}
    Rigidbody Rigidbody{ get; }
    Vector3 TargetPosition { get; set; }
    System.Action CompletedMove { get; set; }

}