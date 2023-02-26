using UnityEngine;
using Zenject;

public class TeleportWall : MonoBehaviour {
    private ISpawnPoisition _spawnPoisition;

    [Inject]
	private void Construct(ISpawnPoisition spawnPoisition)
	{
		_spawnPoisition = spawnPoisition;
	}

	private void OnTriggerEnter(Collider other)
	{
		var player = other.GetComponent<Player>();
        if (player != null)
        {
			player.OnPlayerTeleport();
			player.transform.position = _spawnPoisition.GetSpawnPosition();
        }
    }
}
