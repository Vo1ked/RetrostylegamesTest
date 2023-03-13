using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "My Game/PlayerStats")]
public class PlayerStats : ScriptableObject {

    public Heals Heals;
    [Space]
    public Mana Mana;
    [Space]
    public float MoveSpeed;
    public float RotationSpeed;
    public float MaxUpRotation = -90;
    public float MaxDownRotation = 40;

    public List<Ability> Abilities;
}
[Serializable]
public class Heals
{
    public int MaxHeals;
    public int StartHeals;
    [NonSerialized] private int _currentHeals;
    public int CurrentHeals
    {
        get { return _currentHeals; }
        set
        {
            _currentHeals = Mathf.Clamp(value, 0, MaxHeals);
            HealsChanged?.Invoke(_currentHeals);
        }
    }
    public event Action<int> HealsChanged;
}
[Serializable]
public class Mana
{
    public int MaxMana;
    public int StartMana;
    [System.NonSerialized] private int _currentMana;

    public int CurrentMana
    {
        get { return _currentMana; }
        set
        {
            _currentMana = Mathf.Clamp(value, 0, MaxMana);
            CurrentManaChanged?.Invoke(_currentMana);
        }
    }
    public event Action<int> CurrentManaChanged;

}
