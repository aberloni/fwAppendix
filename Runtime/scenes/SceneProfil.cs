using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace fwp.scenes
{
    /// <summary>
    /// associer autour d'une UID un ensemble de scene
    /// multi layering scenes
    /// </summary>
    public class SceneProfil
    {
        public string uid = string.Empty;

        public List<string> layers = new List<string>();
        public List<string> deps = new List<string>();

        Scene[] _buffScenes;

        /// <summary>
        /// ingame, want to load a scene
        /// </summary>
        public SceneProfil(string uid)
        {
            uid = extractUid(uid);

            var paths = getPaths(uid);

            // filter paths

            //Debug.Log(GetType()+" : "+ uid);

            for (int i = 0; i < paths.Count; i++)
            {
                //Debug.Log(paths[i]);

                if(checkPathIgnore(paths[i]))
                {
                    //Debug.Log("ignored: " + paths[i]);
                    paths.RemoveAt(i);
                    i--;
                }
            }

            if (paths.Count <= 0) return;

            setup(uid, paths);
        }

        public bool match(SceneProfil sp)
        {
            return sp.uid == uid;
        }

        public bool isValid() => this.uid.Length > 0;

        void setup(string uid, List<string> paths)
        {
            if (uid.ToLower().Contains("SceneManagement"))
            {
                Debug.LogError("invalid uid : " + uid);
                return;
            }

            //makes it valid
            this.uid = uid;

            Debug.Assert(paths.Count > 0, uid + " needs paths");

            for (int i = 0; i < paths.Count; i++)
            {
                paths[i] = SceneTools.removePathBeforeFile(paths[i]);
            }

            this.layers = reorderLayers(paths);

            solveDeps();
        }

        virtual protected List<string> reorderLayers(List<string> paths)
        {
            // this will take the main scene
            // and push it front of array
            // to be loaded first

            int index = -1;
            for (int i = 0; i < paths.Count; i++)
            {
                if(paths[i] == uid)
                {
                    index = i;
                }
            }

            List<string> output = new List<string>();

            if (index > 0)
            {
                paths.Remove(uid);
                output.Add(uid);
            }
            
            output.AddRange(paths);

            return output;
        }

        virtual protected List<string> getPaths(string uid)
        {
            var paths = SceneTools.getScenesPathsOfCategory(uid);
            //var paths = SceneTools.getScenesPathsOfCategory(uid);
            return paths;
        }

        /// <summary>
        /// remove some pattern
        /// </summary>
        virtual protected bool checkPathIgnore(string path)
        {
            return false;
        }

        /// <summary>
        /// beeing able to solve uids differently
        /// like : scene-name_layer => scene-name
        /// </summary>
        virtual protected string extractUid(string path)
        {
            path = SceneTools.removePathBeforeFile(path);

            // scene-name_layer => scene-name
            if (path.IndexOf('_') > 0)
            {
                return path.Substring(0, path.IndexOf('_'));
            }

            return path;
        }

        /// <summary>
        /// pile de toutes les scènes qui seront a charger au runtime
        /// </summary>
        virtual public void solveDeps()
        {
            deps.Clear();
        }

        public bool isLoaded()
        {
            if (layers.Count <= 0) return false;
            return SceneManager.GetSceneByName(layers[0]).isLoaded;
        }

#if UNITY_EDITOR
        public void editorLoad(bool additive)
        {
            solveDeps();

            Debug.Log($"SceneProfil:editorLoad <b>{uid}</b> ; layers x{layers.Count} & deps x{deps.Count}");

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
            //solveDeps();

            for (int i = 0; i < layers.Count; i++)
            {
                SceneLoaderEditor.unloadScene(layers[i]);
            }

            for (int i = 0; i < deps.Count; i++)
            {
                SceneLoaderEditor.unloadScene(deps[i]);
            }

            //var sc = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByName(layers[0]);
            //UnityEditor.SceneManagement.EditorSceneManager.CloseScene(sc, true);
        }
#endif

        public void buildLoad(Action<Scene> onLoadedCompleted)
        {
            //solveDeps();

            loadDeps(() =>
            {
                loadLayers((Scene mainScene) =>
                {
                    onLoadedCompleted?.Invoke(mainScene);
                });
            });

        }

        void loadDeps(Action onCompletion)
        {
            if (deps.Count <= 0)
            {
                Debug.LogWarning("deps array is empty ?");
                onCompletion?.Invoke();
                return;
            }

            Debug.Log("loading deps x" + deps.Count);

            SceneLoader.loadScenes(deps.ToArray(), (Scene[] scs) =>
            {
                onCompletion?.Invoke();
            });
        }

        void loadLayers(Action<Scene> onCompletion)
        {

            if (layers.Count <= 0)
            {
                Debug.LogWarning("layers array is empty ?");
                onCompletion?.Invoke(default(Scene));
                return;
            }

            Debug.Log("loading layers x" + deps.Count);

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
                onCompletion?.Invoke(extractMainScene());
            });
        }

        public void buildUnload(System.Action onUnloadCompleted)
        {
            solveDeps();

            Debug.Log(GetType()+" : " + uid + " is <b>unloading</b>");

            SceneLoader.unloadScenes(layers.ToArray(), onUnloadCompleted);
        }

        public Scene extractMainScene()
        {
            Debug.Assert(_buffScenes.Length > 0, "buff scenes must not be empty here");
            Debug.Assert(_buffScenes[0].IsValid());

            return _buffScenes[0];
        }

        virtual public string editor_getButtonName() => uid+" (x"+layers.Count+")";

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
