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
        static public bool verbose = false;

        public string uid = string.Empty; // is a category, base path

        public string path;

        //these are only scene names (no ext, no path)
        public List<string> layers = new List<string>();
        public List<string> deps = new List<string>();

        Scene[] _buffScenes;

        /// <summary>
        /// ingame, want to load a scene
        /// </summary>
        public SceneProfil(string categoryUid)
        {
            this.uid = string.Empty; // invalid

            categoryUid = extractUid(categoryUid);
            Debug.Assert(categoryUid.Length > 0, "empty uid ? given : "+ categoryUid);

            var paths = getPaths(categoryUid);
            Debug.Assert(paths.Count > 0, "empty paths[] ? uid : " + categoryUid);

            // filter paths

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

            if (paths.Count <= 0)
            {
                Debug.LogWarning(categoryUid + " has no remaining paths after filtering ?");
                return;
            }

            //Debug.Log(getStamp() + " created");

            this.uid = setup(categoryUid, paths);
        }

        public string parentFolder
        {
            get
            {
                string _path = path;

                //Debug.Log(path);

                // remove scene name
                _path = _path.Substring(0, _path.LastIndexOf('/'));

                // remove everything up to folder parent
                _path = _path.Substring(_path.LastIndexOf('/') + 1);

                return _path;
            }
        }

        public bool match(SceneProfil sp)
        {
            return sp.uid == uid;
        }

        public bool isValid() => this.uid.Length > 0;

        string setup(string setupUid, List<string> paths)
        {
            if (uid.ToLower().Contains("SceneManagement"))
            {
                Debug.LogError("invalid uid : " + uid);
                return string.Empty;
            }

            // prebuff for paths fetching
            this.uid = setupUid;

            // default
            path = paths[0];

            Debug.Assert(paths.Count > 0, setupUid + " needs paths");

            for (int i = 0; i < paths.Count; i++)
            {
                // keep shortest path
                if (path.Length > paths[i].Length) path = paths[i];

                paths[i] = SceneTools.removePathBeforeFile(paths[i]);
            }

            // push main scene first
            this.layers = reorderLayers(paths);

            solveDeps();

            return setupUid; // length>0 makes it valid
        }

        /// <summary>
        /// this will take the main scene
        /// and push it front of array
        /// to be loaded first
        /// </summary>
        virtual protected List<string> reorderLayers(List<string> paths)
        {

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

            if(verbose) Debug.Log($"SceneProfil:editorLoad <b>{uid}</b> ; layers x{layers.Count} & deps x{deps.Count}");

            UnityEditor.SceneManagement.OpenSceneMode mode = UnityEditor.SceneManagement.OpenSceneMode.Single;
            if (additive) mode = UnityEditor.SceneManagement.OpenSceneMode.Additive;

            

            //first load base scene
            string baseScene = layers[0];
            if (verbose) Debug.Log($"SceneProfil:loading base scene {baseScene}");
            SceneLoaderEditor.loadScene(baseScene, mode);

            //load additive others
            for (int i = 1; i < layers.Count; i++)
            {
                if (verbose) Debug.Log($"SceneProfil:loading layer:{layers[i]}");
                SceneLoaderEditor.loadScene(layers[i]);
            }

            //load deps
            for (int i = 0; i < deps.Count; i++)
            {
                if (verbose) Debug.Log($"SceneProfil:loading layer:{deps[i]}");
                SceneLoaderEditor.loadScene(deps[i]);
            }

            //lock by editor toggle
            //HalperEditor.upfoldNodeHierarchy();
        }
        
        public void editorUnload()
        {
            //solveDeps();
            
            if (verbose) Debug.Log($"SceneProfil:unload");

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

        virtual protected float getDebugLoadDelay() => 0f;

        public void buildLoad(Action<Scene> onLoadedCompleted)
        {
            //solveDeps();

            if(verbose) Debug.Log(getStamp() + " builload");

            loadDeps(() =>
            {
                loadLayers(() =>
                {
                    Scene parentScene = extractMainScene();
                    onLoadedCompleted?.Invoke(parentScene);
                });
            });

        }

        void loadDeps(Action onCompletion)
        {
            if (deps.Count <= 0)
            {
                Debug.LogWarning(getStamp()+" deps array is empty ?");
                onCompletion.Invoke();
                return;
            }

            if (verbose)
            {
                Debug.Log(getStamp() + " loading deps x" + deps.Count);
                for (int i = 0; i < deps.Count; i++) Debug.Log(getStamp() + " dep:" + deps[i]);
            }

            float delay = 0f;

#if UNITY_EDITOR
            delay = getDebugLoadDelay();
#endif

            SceneLoader.loadScenes(deps.ToArray(), (Scene[] scs) =>
            {
                onCompletion.Invoke();
            }, delay);
        }

        void loadLayers(Action onCompletion)
        {

            if (layers.Count <= 0)
            {
                Debug.LogWarning(getStamp() + " layers array is empty ?");
                onCompletion.Invoke();
                return;
            }

            if (verbose)
            {
                Debug.Log(getStamp() + " loading layers x" + layers.Count);
                for (int i = 0; i < layers.Count; i++) Debug.Log(getStamp() + " layer:" + layers[i]);
            }

            SceneLoader.loadScenes(layers.ToArray(), (Scene[] scs) =>
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

                    //Scene main = extractMainScene();
                    //Debug.Assert(main.IsValid(), getStamp()+" extracted scene : " + main + " is not valid");

                    onCompletion.Invoke();
                });
        }

        public void buildUnload(System.Action onUnloadCompleted)
        {
            solveDeps();

            Debug.Log(getStamp()+" : " + uid + " is <b>unloading</b>");

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

        public string stringify()
        {
            string output = uid;
            if(!string.IsNullOrEmpty(path)) output += " & " + path;
            return output;
        }

        string getStamp()
        {
            return "{SceneProfil} " + stringify();
        }
    }

}
