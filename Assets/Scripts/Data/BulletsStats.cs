using RetroStyleGamesTest.Abillity.Implementation;
using RetroStyleGamesTest.Units;
using UnityEngine;

namespace RetroStyleGamesTest.Data
{
    [CreateAssetMenu(fileName = "BulletsStats", menuName = "My Game/Shooter/BulletsStats")]
    public class BulletsStats : ScriptableObject
    {
        [SerializeField] private new string name;
        public Bullet Bullet;
        [HideInInspector] public GameObject Shooter;
        public float Speed;
        public HitInfo HitInfo;
    }
}