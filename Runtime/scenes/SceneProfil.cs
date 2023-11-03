using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

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
        /// force add is not available in builds
        /// if given scene name 
        /// (true)  have prefix before : context_name_layer
        /// (false) or just : name_layer
        /// </summary>
        public SceneProfil(string categoryUid, bool hasContextInName = false)
        {
            this.uid = string.Empty; // invalid

            string solvedCategoryUid = extractUid(categoryUid, hasContextInName);
            Debug.Assert(solvedCategoryUid.Length > 0, "empty uid ? given : "+ solvedCategoryUid);

            Debug.Log(categoryUid + " ? " + solvedCategoryUid);

            var paths = filterAllPaths(solvedCategoryUid, true);
            
            if (paths.Count <= 0)
            {
                Debug.LogWarning(solvedCategoryUid + " has no remaining paths after filtering ?");
                return;
            }

            //Debug.Log(getStamp() + " created");

            this.uid = setup(solvedCategoryUid, paths);
        }

        /// <summary>
        /// extract all suited scenes from assetdatabase
        /// </summary>
        List<string> filterAllPaths(string categoryUid, bool removeExt = false)
        {

            var paths = getPaths(categoryUid, removeExt);
            Debug.Assert(paths.Count > 0, "empty paths[] ? uid : " + categoryUid);

            // filter paths

            for (int i = 0; i < paths.Count; i++)
            {
                //Debug.Log(paths[i]);

                if (checkPathIgnore(paths[i]))
                {
                    //Debug.Log("ignored: " + paths[i]);
                    paths.RemoveAt(i);
                    i--;
                }
            }

            return paths;
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

        /// <summary>
        /// force add = force adding all target scene into build settings
        /// </summary>
        virtual protected List<string> getPaths(string uid, bool removeExt = false)
        {
            var paths = SceneTools.getScenesPathsOfCategory(uid, removeExt);
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
        /// scene name must always be the last or the n-1
        /// like : scene-name_layer => scene-name
        /// 
        /// bool context : if scene name have prefix
        /// context_scene_layer => context_scene
        /// </summary>
        virtual protected string extractUid(string path, bool hasContext)
        {
            path = SceneTools.removePathBeforeFile(path);

            Debug.Log(hasContext + "&"+path);

            string[] split = path.Split('_');
            int underscoreCount = split.Length - 1;

            if (underscoreCount > 2)
                hasContext = true;

            if(!hasContext)
            {
                // scene-name_layer => scene-name
                // only one _ present
                if (underscoreCount == 1)
                {
                    path = path.Substring(0, path.IndexOf('_'));
                }

                return path;
            }

            // context
            if (underscoreCount > 0)
            {
                // context_name
                
                // rem context prefix
                path = path.Substring(path.IndexOf('_')+1);

                // context_name_layer
                // has layer ? 
                if (underscoreCount > 1)
                {
                    // rem layer
                    path = path.Substring(0, path.LastIndexOf('_'));
                }
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

        void forceAddToBuildSettings()
        {
#if UNITY_EDITOR
            List<EditorBuildSettingsScene> tmp = new List<EditorBuildSettingsScene>();

            // keep existing
            if(EditorBuildSettings.scenes != null)
            {
                if(EditorBuildSettings.scenes.Length > 0)
                    tmp.AddRange(EditorBuildSettings.scenes);
            }
            
            //var scenes = SceneTools.getProjectAssetScenesPaths();
            var scenes = filterAllPaths(uid, false); // force adding, NEED extensions

            foreach (string path in scenes)
            {
                // no duplicates
                if (tmp.Exists(x => x.path == path))
                {
                    if (verbose)
                        Debug.LogWarning("duplicate, skipping");

                    continue;
                }

                // /! path NEEDS extension
                tmp.Add(new EditorBuildSettingsScene(path, true));
            }

            if(tmp.Count > 0)
            {
                EditorBuildSettings.scenes = tmp.ToArray();

                for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    Debug.Log("#" + i + " => " + EditorBuildSettings.scenes[i].path);
                }

                if(verbose)
                    Debug.Log("forced added scenes to build settings : x" + EditorBuildSettings.scenes.Length);
            }

#endif
        }

#if UNITY_EDITOR
        public void editorLoad(bool additive, bool forceAddBuildSettings = false)
        {
            // first check that scenes are added to build settings ?
            if (forceAddBuildSettings) forceAddToBuildSettings();

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


            if (verbose) Debug.Log(getStamp() + " builload");

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
