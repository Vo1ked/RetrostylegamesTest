using UnityEngine;
using Zenject;

public class FarthestSpawnPositionFromEnemies : ISpawnType
{
    private EnemySpawner _enemySpawner;

    public SpawnType type => SpawnType.farthestSpawnFromEnemies;

    [Inject]
    private void Construct(EnemySpawner enemySpawner)
    {
        _enemySpawner = enemySpawner;
    }

    public Vector3 GetSpawnPosition()
    {
        return FindFarthestCoordinateFromCoordinatesArray(_enemySpawner.GetEnemiesPosition());
    }

    private Vector3 FindFarthestCoordinateFromCoordinatesArray(Vector3[] coordinates)
    {
        Vector3 farthestCoordinate = Vector3.zero;
        float maxDistance = 0f;

        for (int i = 0; i < coordinates.Length; i++)
        {
            for (int j = i + 1; j < coordinates.Length; j++)
            {
                float distance = Vector3.Distance(coordinates[i], coordinates[j]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    farthestCoordinate = (coordinates[i] + coordinates[j]) / 2f;
                }
            }
        }

        return farthestCoordinate;
    }
}
