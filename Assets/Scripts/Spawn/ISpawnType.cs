using UnityEngine;

public interface ISpawnType
{
	SpawnType type { get; }
	Vector3 GetSpawnPosition();
}
