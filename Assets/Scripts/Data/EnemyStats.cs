using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyStats", menuName = "My Game/EnemyStats")]
public class EnemyStats : ScriptableObject {

	public new string name;
	public Enemy enemy;
	public int Heals;
	public int StartHeals;
	[Space]
	public float MoveSpeed;
	public float RotationSpeed;
	[Space]
	public List<Ability> Abilities;
}
