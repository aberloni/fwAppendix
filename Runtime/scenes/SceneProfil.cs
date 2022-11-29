using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace fwp.scenes
{
    /// <summary>
    /// associer autour d'une UID un ensemble de scene
    /// multi layering scenes
    /// </summary>
    public class SceneProfil
    {
        public string uid;

        public List<string> layers = new List<string>();
        public List<string> deps = new List<string>();

        Scene[] _buffScenes;

        public SceneProfil(string uid)
        {
            if (uid.ToLower().Contains("SceneManagement"))
            {
                Debug.LogError("invalid uid : " + uid);
                return;
            }

            this.uid = uid;

            reload();
        }

        /// <summary>
        /// pile de toutes les scènes qui seront a charger au runtime
        /// </summary>
        public void reload()
        {
            if (layers == null) layers = new List<string>();
            else layers.Clear();

            layers.Add(uid);
        }

#if UNITY_EDITOR
        public void editorLoad(bool additive)
        {
            Debug.Log($"SceneProfil:editorLoad <b>{uid}</b>");

            UnityEditor.SceneManagement.OpenSceneMode mode = UnityEditor.SceneManagement.OpenSceneMode.Single;
            if (additive) mode = UnityEditor.SceneManagement.OpenSceneMode.Additive;

            //first load base scene
            string baseScene = layers[0];
            SceneLoaderEditor.loadScene(baseScene, mode);

            //load additive others
            for (int i = 1; i < layers.Count; i++)
            {
                SceneLoaderEditor.loadScene(layers[i]);
            }

            //load deps
            for (int i = 0; i < deps.Count; i++)
            {
                SceneLoaderEditor.loadScene(deps[i]);
            }

            //lock by editor toggle
            //HalperEditor.upfoldNodeHierarchy();
        }

        public void editorUnload()
        {
            //SceneManager.UnloadSceneAsync(layers[0]);
            var sc = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByName(layers[0]);
            UnityEditor.SceneManagement.EditorSceneManager.CloseScene(sc, true);
        }
#endif

        public void buildLoad(System.Action<Scene> onLoadedCompleted)
        {

            SceneLoader.loadScenes(deps.ToArray(), (Scene[] scs) =>
            {
                SceneLoader.loadScenes(layers.ToArray(),
                (Scene[] scs) =>
                {
                    if (scs.Length <= 0)
                    {
                        Debug.LogError("no scenes returned ?");
                        for (int i = 0; i < layers.Count; i++)
                        {
                            Debug.Log("  " + layers[i]);
                        }
                    }

                    _buffScenes = scs;
                    onLoadedCompleted?.Invoke(extractMainScene());
                });
            });

        }

        public void buildUnload(System.Action onUnloadCompleted)
        {
            Debug.Log(GetType()+" : " + uid + " is <b>unloading</b>");

            SceneLoader.unloadScenes(layers.ToArray(), onUnloadCompleted);
        }

        public Scene extractMainScene()
        {
            Debug.Assert(_buffScenes.Length > 0, "buff scenes must not be empty here");
            Debug.Assert(_buffScenes[0].IsValid());

            return _buffScenes[0];
        }

        /// <summary>
        /// EDITOR
        /// make sure all related scenes are present in build settings
        /// </summary>
        void forcePresenceBuildSettings()
        {

        }

        /// <summary>
        /// RUNTIME
        /// est-ce que ce profil est dispo dans les builds settings
        /// </summary>
        /// <returns></returns>
        bool isAvailableInBuild()
        {
            return true;
        }
    }

}
