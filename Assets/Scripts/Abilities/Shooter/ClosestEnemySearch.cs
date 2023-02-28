using UnityEngine;
using Zenject;

public class ClosestEnemySearch : ITargetSearch
{
    private EnemySpawner _enemySpawner;

    [Inject]
    private void Construct(EnemySpawner enemySpawner)
    {
        _enemySpawner = enemySpawner;
    }
    public Vector3 GetTarget(Vector3 position)
    {
        Enemy closest = null;
        Vector3 enemyPosition = Vector3.positiveInfinity;
        float distance = Mathf.Infinity;

        foreach (var enemy in _enemySpawner.enemies)
        {
            var currentDistance = Vector3.Distance(enemyPosition, enemy.transform.position);
            if (currentDistance < distance)
            {
                closest = enemy;
                enemyPosition = enemy.transform.position;
                distance = currentDistance;
            }
        }

        return closest.transform.position;
    }
}
