using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnPatern", menuName = "My Game/SpawnPatern")]
public class SpawnPatern : ScriptableObject
{
	public float DelayBeforeFirstSpawn;
	public float StartSpawnDelay;
	public float SpawnDelayChangeStep;
	public float MinSpawnDelayRate;
	public float EnemyMultplierSpawnRate;
	public float MaxEnemyMultplierSpawnRate;
	public List<EnemyToSpawn> enemies;

}

[System.Serializable]
public class EnemyToSpawn
{
	public EnemyStats EnemyStats;
	public int BaseSpawnCount;

}