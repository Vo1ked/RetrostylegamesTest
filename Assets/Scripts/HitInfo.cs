using UnityEngine;
[System.Serializable]
public class HitInfo {

	[SerializeField] private int _healsDamage;
	[SerializeField] private int _manaDamage;
    public int HealsDamage { get { return _healsDamage; } }
    public int ManaDamage { get { return _manaDamage; } }

}
