using UnityEngine;

namespace RetroStyleGamesTest.Units
{
    internal interface IMovable
    {
        Rigidbody Rigidbody { get; }
        Vector3 TargetPosition { get; set; }
        System.Action CompletedMove { get; set; }
    }
}