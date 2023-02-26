using System.Collections.Generic;
using UnityEngine;
public class BulletsStats : ScriptableObject
{
    [SerializeField] private string name;
    public GameObject Body;
    public GameObject Shooter;
    public float Speed;
    public int Damage;

    public List<Ability> Abilities;

}