using UnityEngine;

namespace RetroStyleGamesTest.SpawnPosition
{
	public interface ISpawnType
	{
		SpawnType type { get; }
		Vector3 GetSpawnPosition();
	}
}