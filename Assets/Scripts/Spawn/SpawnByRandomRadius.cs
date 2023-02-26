using UnityEngine;

public class SpawnByRandomRadius : ISpawnPoisition {

    public Vector3 GetSpawnPosition()
    {
        Vector3 middlePoint = new Vector3(0, 0, 0);
        float circleRadius = 4;
        var tryCount = 100;
        var tries = 0;

        var result = GetPosition(middlePoint, circleRadius);
        while (!ValidatePosition(result) || tries > tryCount)
        {
            result = GetPosition(middlePoint, circleRadius);
            tries++;
        }
        return result;
    }

    private Vector3 GetPosition(Vector3 middlePoint,float circleRadius)
    {
        var random = Random.insideUnitCircle;
        return new Vector3(middlePoint.x + random.x * circleRadius,
            middlePoint.y,
            middlePoint.z + random.y * circleRadius);
    }

    private bool ValidatePosition(Vector3 position)
    {
        RaycastHit hit;
        return !Physics.SphereCast(position, 1, Vector3.zero, out hit);
    }

}
