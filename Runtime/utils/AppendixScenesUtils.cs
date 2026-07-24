using UnityEngine;
using UnityEngine.SceneManagement;

public static class AppendixScenesUtils
{
    /// <summary>
    /// Computes the average position of all root GameObject transforms in a scene,
    /// excluding outliers (positions too far from the group).
    /// </summary>
    /// <param name="scene">Target scene.</param>
    /// <param name="stdDevThreshold">Max allowed distance from centroid, in std deviations.</param>
    public static Vector3 GetAveragePosition(Scene scene, float stdDevThreshold = 1.5f)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        int count = roots.Length;
        if (count == 0)
            return Vector3.zero;

        Vector3[] positions = new Vector3[count];
        for (int i = 0; i < count; i++)
            positions[i] = roots[i].transform.position;

        Vector3 centroid = Average(positions);

        float[] distances = new float[count];
        float distSum = 0f;
        for (int i = 0; i < count; i++)
        {
            distances[i] = Vector3.Distance(positions[i], centroid);
            distSum += distances[i];
        }
        float meanDist = distSum / count;

        float varianceSum = 0f;
        for (int i = 0; i < count; i++)
        {
            float diff = distances[i] - meanDist;
            varianceSum += diff * diff;
        }
        float stdDev = Mathf.Sqrt(varianceSum / count);
        float maxDist = meanDist + stdDevThreshold * stdDev;

        Vector3 filteredSum = Vector3.zero;
        int filteredCount = 0;
        for (int i = 0; i < count; i++)
        {
            if (distances[i] <= maxDist)
            {
                filteredSum += positions[i];
                filteredCount++;
            }
        }

        if (filteredCount == 0)
            return centroid;

        return filteredSum / filteredCount;
    }

    private static Vector3 Average(Vector3[] points)
    {
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < points.Length; i++)
            sum += points[i];
        return sum / points.Length;
    }
}