using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollisionEventRepeater : MonoBehaviour
{

	public event Action<Collider> TriggerEnter = (Collider) => { };
	public event Action<Collider> TriggerExit = (Collider) => { };

	private void OnTriggerEnter(Collider other)
	{
		TriggerEnter.Invoke(other);
	}
	private void OnTriggerExit(Collider other)
	{
		TriggerExit.Invoke(other);
	}
}
