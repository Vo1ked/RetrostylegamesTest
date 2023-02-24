﻿using UnityEngine;

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
    [Space]
    public float BulletDamage;
    public float BulletSpeed;
    public float reboundChance;




}
