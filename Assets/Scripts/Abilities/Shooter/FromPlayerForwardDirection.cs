using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class FromPlayerForwardDirection : MonoBehaviour, ITargetSearch
{
	private Player _player;

    public Vector3 GetTarget(Vector3 position)
    {
        throw new System.NotImplementedException();
    }

    [Inject]
	private void Construct(Player player)
	{
		_player = player;
	}
	

}
