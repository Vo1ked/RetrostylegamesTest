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
        RaycastHit hit;
        if (Physics.Raycast(user.transform.position, user.transform.forward, out hit, Range))
        {
            var damageble = hit.transform.GetComponent<IDamageble>();
            if (damageble != null)
            {
                damageble.Damage(HitInfo);
                _enemySpawner.Destroy(user.GetComponent<Enemy>());
            }
        }
    }

}
