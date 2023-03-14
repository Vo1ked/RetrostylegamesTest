using UnityEngine;
using Zenject;
using RetroStyleGamesTest.SpawnPosition;

namespace RetroStyleGamesTest.Units
{
	public class TeleportWall : MonoBehaviour
	{
		private SpawnPositionFactory _spawnPoisition;

		[Inject]
		private void Construct(SpawnPositionFactory spawnPoisition)
		{
			_spawnPoisition = spawnPoisition;
		}

		private void OnTriggerEnter(Collider other)
		{
			var player = other.GetComponent<Player>();
			if (player != null)
			{
				player.OnPlayerTeleport();
				player.transform.position = _spawnPoisition.GetSpawnPosition(SpawnType.farthestSpawnFromEnemies);
			}
		}
	}
}
