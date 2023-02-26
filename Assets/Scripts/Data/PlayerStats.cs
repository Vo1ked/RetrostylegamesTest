using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "My Game/PlayerStats")]
public class PlayerStats : ScriptableObject {

    public int Heals;
    public int StartHealsCount;
    [Space]
    public int UltimateCapacity;
    public int StartUltimateCount;
    [Space]
    public float MoveSpeed;
    public float MaxMoveSpped;
    public float RotationSpeed;
    public float MaxUpRotation = -90;
    public float MaxDownRotation = 40;

    public List<Ability> Abilities;





}
