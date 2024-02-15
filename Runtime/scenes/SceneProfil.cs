using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace fwp.scenes
{
    /// <summary>
    /// CONTEXT_SCENE{_LAYER}
    /// associer autour d'une UID un ensemble de scene
    /// multi layering scenes
    /// </summary>
    public class SceneProfil
    {
        static public bool verbose = false;

        string _category; // debug
        string _profilPath; // path to profil

        string context_base; // context ONLY
        string context; // context OR context_scene

        bool _dirty = false;

        public string label => context;
        public string pathToScene => _profilPath;

        /// <summary>
        /// returns path without scene folder
        /// path/to/context/
        /// </summary>
        public string parentPath
        {
            get
            {
                Debug.Assert(!string.IsNullOrEmpty(_profilPath), "no profil path to deduce parent path ? (" + _category + ")");

                string _path = _profilPath;

                //  Assets/Scenes/Resources/resource-engine
                Debug.Assert(_path.IndexOf("/") > 0, "path has no '/' ? " + _path);

                if (_path.IndexOf('/') > 0)
                {
                    // remove scene name
                    _path = _path.Substring(0, _path.LastIndexOf('/'));
                }

                return _path;
            }
        }

        /// <summary>
        /// returns just the name of the parent folder
        /// </summary>
        public string parentFolder
            => parentPath.Substring(parentPath.LastIndexOf('/') + 1);

        //these are only scene names (no ext, no path)
        public List<string> layers;
        public List<string> deps;

        List<SceneAssoc> _assocs_buff;

        /// <summary>
        /// has found anything
        /// </summary>
        public bool isValid()
        {
            if (layers == null) return false;
            return layers.Count > 0;
        }

        /// <summary>
        /// categoryUid is uniq PATH to scenes
        /// OR simply name of a context
        /// 
        /// ingame, want to load a scene
        /// force add to build settings is not available in builds
        /// </summary>
        public SceneProfil(string categoryUid)
        {
            if (categoryUid.Contains("SceneManagement"))
            {
                Debug.LogError("invalid uid : " + categoryUid);
                return;
            }

            // only keep context, remove path
            // path must be deduce by context
            _category = SceneTools.removePathBeforeFile(categoryUid);
            Debug.Log("SceneProfil = " + _category);

            //Debug.Assert(categoryUid.Split("_").Length < 2, categoryUid + " cannot be partial : CONTEXT_SCENE_LAYER");

            extractContext(_category);

            if (string.IsNullOrEmpty(context))
            {
                Debug.LogError("profil : input = " + _category + " not compat");
                return;
            }

            //if (verbose) Debug.Log("solved context : " + context);

            //Debug.Log(categoryUid + " ? " + solvedCategoryUid);

            // this might return null
            // @runtime : if scenes are not present in build settings
            // must give root name of category (no layer)
            var paths = filterAllPaths(true);

            if (paths == null)
            {
                if (verbose) Debug.Log(categoryUid + " paths is null ?");
                return;
            }

            if (paths.Count <= 0)
            {
                if (verbose) Debug.Log(categoryUid + " has no paths ?");
                return;
            }

            // remove non-compat
            paths = filterPaths(paths);

            // solve layers & deps paths
            solvePaths(paths);

            Debug.Assert(!string.IsNullOrEmpty(_profilPath), "profil path must not be null : " + _category);
        }

        public void refresh()
        {
            if (Application.isPlaying)
                return;

            if (_assocs_buff == null)
                _dirty = true;

            if (_dirty)
            {
                fetchAssocs(true);
            }

            _dirty = false;
        }

        public void setDirty()
        {
            _dirty = true;
        }

        /// <summary>
        /// extract all suited scenes from assetdatabase
        /// </summary>
        List<string> filterAllPaths(bool removeExt = false)
        {
            // categoryUid as "folder contains"
            // get all paths to scenes matching category uid
            var paths = getPaths(context_base, removeExt);
            if (paths.Count <= 0)
            {
                Debug.LogWarning($"given base context : <b>{context_base}</b> => empty paths[] (length = 0)");
                Debug.LogWarning("target was not added to build settings ?");
                return null;
            }

            // keep any of the path as reference
            // to gatekeep others
            _profilPath = paths[0];

            // filter paths

            for (int i = 0; i < paths.Count; i++)
            {
                Debug.Log("#" + i + " : " + paths[i]);

                if (!checkPathCompatibility(paths[i]))
                {
                    //Debug.Log("ignored: " + paths[i]);
                    paths.RemoveAt(i);
                    i--;
                }
            }

            return paths;
        }

        public bool match(SceneProfil sp)
        {
            return sp.label == label;
        }

        /// <summary>
        /// returns path to profil
        /// </summary>
        List<string> filterPaths(List<string> paths)
        {
            //if (verbose) Debug.Log("profil: setup(" + context + ") paths x" + paths.Count);

            Debug.Assert(paths.Count > 0, "paths empty ?");

            // clean paths
            for (int i = 0; i < paths.Count; i++)
            {
                paths[i] = SceneTools.removePathBeforeFile(paths[i]);
            }

            // push main scene first
            paths = reorderLayers(paths);

            return paths;
        }

        void solvePaths(List<string> paths)
        {
            if (layers == null) layers = new List<string>();
            layers.Clear();
            layers.AddRange(paths);

            //if (verbose) Debug.Log("profil#" + label + " : found layers x" + layers.Count);

            solveDeps();
        }

        /// <summary>
        /// this will take the main scene
        /// and push it front of array
        /// to be loaded first
        /// </summary>
        virtual protected List<string> reorderLayers(List<string> paths)
        {
            // search for original
            int index = -1;
            for (int i = 0; i < paths.Count; i++)
            {
                if (paths[i] == context_base)
                {
                    index = i;
                }
            }

            List<string> output = new List<string>();

            if (index > 0)
            {
                paths.Remove(context_base); // lose ref
                output.Add(context_base); // add ref in front
            }

            // add others
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
        /// ce path est compat avec ce profil ?
        /// </summary>
        virtual protected bool checkPathCompatibility(string path)
        {
            // both this profil AND given path must share same path
            string copy = path.Substring(0, path.LastIndexOf("/"));
            return copy.Contains(parentPath);
        }

        /// <summary>
        /// trying to extract uid aka "context{_scene}" from path
        /// everything but suffix
        /// 
        /// beeing able to solve uids differently
        /// scene name must always be the last or the n-1
        /// like : scene-name_layer => scene-name
        /// 
        /// bool context : if scene name have prefix
        /// context_scene_layer => context_scene
        /// </summary>
        protected void extractContext(string path)
        {
            context = null;
            context_base = null;

            path = SceneTools.removePathBeforeFile(path);
            string[] split = path.Split('_');

            if (split.Length < 0)
            {
                return;

            }

            context_base = split[0];
            context = context_base;

            if (split.Length > 1)
            {
                context += "_" + split[1];
            }

        }

        /// <summary>
        /// pile de toutes les scènes qui seront a charger au runtime
        /// </summary>
        virtual public void solveDeps()
        {
            if (deps == null) deps = new List<string>();
            deps.Clear();
        }

        public bool isLoaded()
        {
            if (layers.Count <= 0) return false;
            return SceneManager.GetSceneByName(layers[0]).isLoaded;
        }

        public void checkAddToBuildSettings(bool force)
        {
            if (force)
                forceAddToBuildSettings();
        }

        void forceAddToBuildSettings()
        {
#if UNITY_EDITOR
            List<EditorBuildSettingsScene> tmp = new List<EditorBuildSettingsScene>();

            // keep existing
            if (EditorBuildSettings.scenes != null)
            {
                if (EditorBuildSettings.scenes.Length > 0)
                    tmp.AddRange(EditorBuildSettings.scenes);
            }

            //var scenes = SceneTools.getProjectAssetScenesPaths();
            var scenes = filterAllPaths(false); // force adding, NEED extensions

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

            if (tmp.Count > 0)
            {
                //assign
                EditorBuildSettings.scenes = tmp.ToArray();

                if (verbose)
                {
                    for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                    {
                        Debug.Log("#" + i + " => " + EditorBuildSettings.scenes[i].path);
                    }
                }

                if (verbose)
                    Debug.Log("forced added scenes to build settings : x" + EditorBuildSettings.scenes.Length);
            }

#endif
        }

#if UNITY_EDITOR

        /// <summary>
        /// additive : is first scene additivly added
        /// </summary>
        public void editorLoad(bool additive, bool forceAddBuildSettings = false)
        {
            // first check that scenes are added to build settings ?
            if (forceAddBuildSettings) forceAddToBuildSettings();

            solveDeps();

            if (verbose) Debug.Log($"SceneProfil:editorLoad <b>{label}</b> ; layers x{layers.Count} & deps x{deps.Count}");

            UnityEditor.SceneManagement.OpenSceneMode mode = UnityEditor.SceneManagement.OpenSceneMode.Single;
            if (additive) mode = UnityEditor.SceneManagement.OpenSceneMode.Additive;

            // first : load base scene
            // additive check : might wanna replace context
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

        /// <summary>
        /// create a virtual delay after loadings layers & deps
        /// </summary>
        virtual protected float getDebugLoadDelay() => 0f;

        public void buildLoad(Action<SceneProfil> onLoadedCompleted)
        {
            //solveDeps();


            if (verbose) Debug.Log(getStamp() + " builload");

            loadDeps(() =>
            {
                loadLayers(() =>
                {
                    //Scene? parentScene = extractMainScene(false);
                    onLoadedCompleted?.Invoke(this);
                });
            });

        }

        void loadDeps(Action onCompletion)
        {
            if (deps.Count <= 0)
            {
                //Debug.LogWarning(getStamp() + " deps array is empty ?");
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

            SceneLoader.loadScenes(deps.ToArray(), (SceneAssoc[] scs) =>
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

            if (_assocs_buff == null) _assocs_buff = new List<SceneAssoc>();

            SceneLoader.loadScenes(layers.ToArray(), (SceneAssoc[] scs) =>
                {
                    if (scs.Length <= 0)
                    {
                        Debug.LogError("no scenes returned ?");
                        for (int i = 0; i < layers.Count; i++)
                        {
                            Debug.Log("  " + layers[i]);
                        }
                    }

                    _assocs_buff.AddRange(scs);

                    //Scene main = extractMainScene();
                    //Debug.Assert(main.IsValid(), getStamp()+" extracted scene : " + main + " is not valid");

                    onCompletion.Invoke();
                });
        }

        public void buildUnload(System.Action onUnloadCompleted)
        {
            solveDeps();

            Debug.Log(getStamp() + " : " + label + " is <b>unloading</b>");

            SceneLoader.unloadScenes(layers.ToArray(), onUnloadCompleted);
        }

        List<SceneAssoc> fetchAssocs(bool force)
        {
            if (_assocs_buff == null)
                _assocs_buff = new List<SceneAssoc>();

            if (_assocs_buff.Count <= 0 || force)
            {
                _assocs_buff.Clear();

                _assocs_buff.AddRange(SceneAssoc.solveScenesAssocs(layers.ToArray()));
                _assocs_buff.AddRange(SceneAssoc.solveScenesAssocs(deps.ToArray()));

                if (verbose)
                    Debug.Log("assocs x" + _assocs_buff.Count);
            }

            return _assocs_buff;
        }

        public GameObject extractRoot(string sceneName, string rootName)
        {
            Scene? sc = extractScene(sceneName);

            if (sc == null)
                return null;

            foreach (var root in sc.Value.GetRootGameObjects())
            {
                if (root.name.Contains(rootName))
                    return root;
            }

            return null;
        }

        public Scene? extractScene(string nm)
        {
            refresh();

            if (_assocs_buff == null)
                return null;

            if (_assocs_buff.Count <= 0)
            {
                return null;
            }

            foreach (var assoc in _assocs_buff)
            {
                if (assoc.handle.name.Contains(nm))
                {
                    return assoc.handle;
                }
            }

            return null;
        }

        public Scene? extractMainScene()
        {
            refresh();

            if (_assocs_buff == null)
                return null;

            if (_assocs_buff.Count <= 0)
            {
                return null;
            }

            if (_assocs_buff[0].isLoaded())
            {
                return _assocs_buff[0].handle;
            }

            return null;
        }

        virtual public string editor_getButtonName() => label + " (x" + layers.Count + ")";

        public string stringify()
        {
            string output = label;
            if (!string.IsNullOrEmpty(_profilPath)) output += " & profil path : " + _profilPath;
            if (layers != null) output += " lyr[x" + layers.Count + "]";
            return output;
        }

        string getStamp()
        {
            return "{SceneProfil} " + stringify();
        }



    }


}

public struct SceneAssoc
{
    public string path;
    public Scene handle;
}