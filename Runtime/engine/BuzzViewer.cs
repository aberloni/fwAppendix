using UnityEngine;

namespace fwp.buzz
{
    /// <summary>
    /// only present when any buzz is active
    /// empty buzz[] will destroy this viewer
    /// </summary>
    public class BuzzViewer : MonoBehaviour
    {
        static public BuzzViewer fetch()
        {
            // no VIEW if buzz in not active
            if (Buzz.instance == null) return null;

            var bv = FindAnyObjectByType<BuzzViewer>();
            if (bv != null) return bv;
            return new GameObject("~buzz").AddComponent<BuzzViewer>();
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void LateUpdate()
        {
            if (!Buzz.instance.isLocking()) // itself
                GameObject.Destroy(gameObject);
        }

        const float w = 300f;
        private void OnGUI()
        {
            GUI.Label(new Rect(Screen.width - w, 30, w, 800), Buzz.instance.stringify());
        }
    }
}