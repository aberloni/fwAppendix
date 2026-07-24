using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace fwp.appendix.utils
{

    public class AppendixSceneViewUtils
    {

#if UNITY_EDITOR
        /// <summary>
        /// Frames the active Scene view on the given position.
        /// </summary>
        /// <param name="position">World position to frame.</param>
        /// <param name="size">Zoom size of the framed view.</param>
        public static void FrameSceneView(Vector3 position, float size = 10f)
        {
            SceneView view = SceneView.lastActiveSceneView;
            if (view == null)
                return;

            view.LookAt(position, view.rotation, size);
        }

        /// <summary>
        /// Frames the active Scene view so all given positions are visible, excluding outliers.
        /// </summary>
        /// <param name="positions">World positions to frame.</param>
        /// <param name="stdDevThreshold">Max allowed distance from centroid, in std deviations.</param>
        /// <param name="padding">Extra margin added around the bounds.</param>
        public static void FrameSceneView(Vector3[] positions, float stdDevThreshold = 1.5f, float padding = 1f)
        {
            SceneView view = SceneView.lastActiveSceneView;
            if (view == null || positions == null || positions.Length == 0)
                return;

            Vector3 centroid;
            Vector3[] filtered = FilterOutliers(positions, stdDevThreshold, out centroid);
            if (filtered.Length == 0)
                filtered = positions;

            Bounds bounds = new Bounds(filtered[0], Vector3.zero);
            for (int i = 1; i < filtered.Length; i++)
                bounds.Encapsulate(filtered[i]);

            bounds.Expand(padding);
            view.Frame(bounds, false);
        }

        /// <summary>
        /// Frames the active Scene view so all root GameObject transforms in a scene are visible,
        /// excluding outliers.
        /// </summary>
        /// <param name="scene">Target scene.</param>
        /// <param name="stdDevThreshold">Max allowed distance from centroid, in std deviations.</param>
        /// <param name="padding">Extra margin added around the bounds.</param>
        public static void FrameSceneView(Scene scene, float stdDevThreshold = 1.5f, float padding = 1f)
        {
            var pos = CollectPositions(new[] { scene });
            Debug.Log(pos.Length);
            FrameSceneView(pos, stdDevThreshold, padding);
        }

        /// <summary>
        /// Frames the active Scene view so all root GameObject transforms across multiple scenes
        /// are visible, excluding outliers.
        /// </summary>
        /// <param name="scenes">Target scenes.</param>
        /// <param name="stdDevThreshold">Max allowed distance from centroid, in std deviations.</param>
        /// <param name="padding">Extra margin added around the bounds.</param>
        public static void FrameSceneView(Scene[] scenes, float stdDevThreshold = 1.5f, float padding = 1f)
        {
            FrameSceneView(CollectPositions(scenes), stdDevThreshold, padding);
        }
#endif

        /// <summary>
        /// Returns positions with outliers removed (distance from centroid within stdDevThreshold).
        /// </summary>
        private static Vector3[] FilterOutliers(Vector3[] positions, float stdDevThreshold, out Vector3 centroid)
        {
            int count = positions.Length;
            centroid = Average(positions);
            if (count == 0)
                return positions;

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

            List<Vector3> filtered = new List<Vector3>(count);
            for (int i = 0; i < count; i++)
            {
                if (distances[i] <= maxDist)
                    filtered.Add(positions[i]);
            }
            return filtered.ToArray();
        }

        private static Vector3 Average(Vector3[] points)
        {
            Vector3 sum = Vector3.zero;
            for (int i = 0; i < points.Length; i++)
                sum += points[i];
            return sum / points.Length;
        }

        private static Vector3[] CollectPositions(Scene[] scenes)
        {
            List<Vector3> positions = new List<Vector3>();
            if (scenes == null)
                return positions.ToArray();

            for (int s = 0; s < scenes.Length; s++)
            {
                GameObject[] roots = scenes[s].GetRootGameObjects();
                for (int i = 0; i < roots.Length; i++)
                    positions.Add(roots[i].transform.position);
            }
            return positions.ToArray();
        }

    }

}