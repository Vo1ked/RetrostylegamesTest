using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "SelfDestroyMeleeAttack", menuName = "My Game/Ability/SelfDestroyMeleeAttack")]
public class SelfDestroyMeleeAttack : Ability, IAttackAbillity {

    public override Specialization Specialization => Specialization.Attack;
    public override WorkType WorkType => WorkType.@override;
    [SerializeField] private float ReloadTime;
    [SerializeField] private float Range = 2;
    public HitInfo HitInfo;


    float IAttackAbillity.ReloadTime => ReloadTime;
    float IAttackAbillity.AttackRange => Range;

    private EnemySpawner _enemySpawner;
    [Inject]
    private void Construct(EnemySpawner enemySpawner)
    {
        _enemySpawner = enemySpawner;
    }

    public override void Execute(GameObject user, params object[] parameters)
    {
        Debug.LogError($"SelfDestroyMeleeAttack {user.name}");
        RaycastHit hit;
        if (Physics.Raycast(user.transform.position, user.transform.forward, out hit, Range))
        {

            var damageble = hit.transform.GetComponent<IDamageble>();
            Debug.LogError($"SelfDestroyMeleeAttack 1 {hit.collider.name}  damageble {damageble == null} enemy  {user.name}");

            if (damageble != null)
            {
                Debug.LogError($"SelfDestroyMeleeAttack 2{user.name}");

                damageble.Damage(HitInfo);
                _enemySpawner.SelfDestroy(user.GetComponent<Enemy>());
            }
        }
    }

}
