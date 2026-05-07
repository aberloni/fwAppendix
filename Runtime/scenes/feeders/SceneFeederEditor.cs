using UnityEngine;

namespace fwp.scenes.feeder
{
    /// <summary>
    /// a feeder that will work only in editor
    /// </summary>
    public class SceneFeederEditor : SceneLoaderFeederBase
    {
        [Header("#UNITY_EDITOR")]
        public FeederData scenes;

        protected override void solveNames()
        {
            if (isEditor())
            {
                Debug.LogWarning($"feeder:<b>{GetType()}</b>");
                addFeederData(scenes);
            }
        }

        virtual protected bool isEditor()
        {
            return Application.isEditor;
        }

    }
}
