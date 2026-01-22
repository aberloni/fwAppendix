using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.scenes
{
    /// <summary>
    /// base tooling for injection
    /// </summary>
    abstract public class SceneLoaderFeederBase : MonoBehaviour
    {
        List<string> scene_names;
        SceneLoaderRunner runner;

        public bool isFeeding() => runner != null;

        /// <summary>
        /// starts feed process
        /// contextCall is meant to filter if feeder must be called again
        /// </summary>
        public void feed()
        {
            // Debug.Log(GetType() + "::feed()", transform);

            if (scene_names == null) scene_names = new();
            scene_names.Clear();
            solveNames();

            //Debug.Log(EngineObject.getStamp(this) + " now feeding "+nms.Length+" names", transform);
            //for (int i = 0; i < nms.Length; i++) { Debug.Log("  L " + nms[i]);}

            runner = SceneLoader.loadScenes(scene_names.ToArray(), (assocs) =>
            {
                //Debug.Log("feed destroy");
                GameObject.Destroy(this);
            });
        }

        protected void solveMultipleFeeders()
        {
            //check si on doit garder l'objet qui porte les feeders
            MonoBehaviour[] monos = gameObject.GetComponents<MonoBehaviour>();
            if (monos.Length == 1 && monos[0] == this)
            {
                GameObject.Destroy(gameObject);
            }
            else
            {
                GameObject.Destroy(this);
            }
        }

        private void OnDestroy()
        {
            runner = null;
            //Debug.Log(EngineObject.getStamp(this) + " done feeding !");
        }

        /// <summary>
        /// use add() helpers
        /// </summary>
        abstract protected void solveNames();

        protected void addNoPrefix(string nm) => addWithPrefix(string.Empty, nm);
        protected void addNoPrefix(string[] nms) => addWithPrefix(string.Empty, nms);

        protected void addWithPrefix(string prefix, string nm)
        {
            addWithPrefix(prefix, new string[] { nm });
        }

        /// <summary>
        /// prefix-
        /// </summary>
        protected void addWithPrefix(string prefix, string[] names)
        {
            // no content to add ?
            if (names == null) return;
            if (names.Length <= 0) return;

            // prefix is always : [name]-
            if (!string.IsNullOrEmpty(prefix) && !prefix.EndsWith("-")) prefix += "-";

            for (int i = 0; i < names.Length; i++)
            {
                scene_names.Add(prefix + names[i]);
            }
        }

        public string[] getNames()
        {
            return scene_names.ToArray();
        }

    }
}