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

        string _profilDefaultScenePath; // path to first scene found
        public string pingScenePath => _profilDefaultScenePath;

        string _profilPath; // path to profil
        public string parentPath => _profilPath;

        // path : [context]_scene_layer
        string context_base; // context ONLY
        string context; // context OR context_scene

        bool _dirty = false;

        public string label => context;

        //these are only scene names (no ext, no path)
        public List<string> layers; // additionnal content of same profil
        public List<string> deps; // other contextual scenes needed for this profil
        public List<string> statics; // scene that won't be unload

        List<SceneAssoc> _assocs_buff;

        /// <summary>
        /// has found anything
        /// </summary>
        public bool hasContent()
        {

            if (layers == null) return false;
            return layers.Count > 0;
        }

        /// <summary>
        /// any of the layer within has filter contains
        /// </summary>
        public bool matchFilter(string filter)
        {
            if (string.IsNullOrEmpty(filter)) return true;
            if (!hasContent()) return false;

            filter = filter.ToLower();

            if (!label.ToLower().Contains(filter))
            {
                return false;
            }

            foreach (var l in layers)
            {
                if (l.ToLower().Contains(filter))
                {
                    return true;
                }
            }

            return false;
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
            if (verbose) Debug.Log(" + SceneProfil <b>" + _category + "</b>");

            //Debug.Assert(categoryUid.Split("_").Length < 2, categoryUid + " cannot be partial : CONTEXT_SCENE_LAYER");

            extractContext(_category);

            if (string.IsNullOrEmpty(context))
            {
                Debug.LogError("/! profil : input = " + _category + " not compat");
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
                if (verbose) Debug.Log(categoryUid + " : paths is null ?");
                return;
            }

            // remove non-compat
            paths = filterPaths(paths);

            // solve layers & deps paths
            // adds deps
            if (layers == null) layers = new List<string>();
            layers.Clear();
            layers.AddRange(paths);

            if (verbose) Debug.Log(categoryUid + " : layers x " + layers.Count + ", out of x " + paths.Count + " paths");

            // nothing here
            // but context might want to add stuff
            solveDeps();
            solveStatics();

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

        void solveProfilPath(string refPath)
        {
            Debug.Assert(!string.IsNullOrEmpty(refPath), "no ref path given ?");
            Debug.Assert(refPath.IndexOf("/") > 0, "path has no '/' ?");

            _profilDefaultScenePath = refPath;

            // keep any of the path as reference
            // to gatekeep others
            // remove scene name, keep only path
            _profilPath = refPath.Substring(0, refPath.LastIndexOf("/"));

            if (verbose) Debug.Log("profil ref path (compatibility) : " + _profilPath);
        }

        /// <summary>
        /// extract all suited scenes from assetdatabase
        /// </summary>
        List<string> filterAllPaths(bool removeExt = false)
        {

            // gets ALL paths containing this cUID
            // checks if categoryUid is contains in scenes path
            var paths = getPaths(context, removeExt);
            if (paths.Count <= 0)
            {
                Debug.LogWarning($"given base context : <b>{context}</b> => empty paths[] (length = 0)");
                Debug.LogWarning("target context was <color=red>not added to build settings</color> ?");
                return null;
            }

            solveProfilPath(paths[0]);

            // filter paths

            for (int i = 0; i < paths.Count; i++)
            {
                bool toRemove = !checkPathCompatibility(paths[i]);

                if (verbose) Debug.Log("#" + i + " : " + paths[i] + " : removed?" + toRemove);

                if (toRemove)
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

            return paths;
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

        virtual public void solveStatics()
        {
            if (statics == null) statics = new List<string>();
            statics.Clear();
        }

        public bool isLoaded()
        {
            if (layers.Count <= 0) return false;
            return SceneManager.GetSceneByName(layers[0]).isLoaded;
        }

#if UNITY_EDITOR

        void forceAddToBuildSettings()
        {
            List<EditorBuildSettingsScene> tmp = new List<EditorBuildSettingsScene>();

            // keep existing
            if (EditorBuildSettings.scenes != null)
            {
                if (EditorBuildSettings.scenes.Length > 0)
                    tmp.AddRange(EditorBuildSettings.scenes);
            }

            var scenes = filterAllPaths(false); // gather linked scenes

            if (scenes.Count <= 0)
            {
                if (verbose)
                    Debug.LogWarning("no scenes returned after filtering ?");

                return;
            }

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

            if (tmp.Count <= 0)
            {
                if (verbose)
                    Debug.LogWarning("nothing to add to build settings ?");

                return;
            }

            //assign
            EditorBuildSettings.scenes = tmp.ToArray();

            if (verbose)
            {
                Debug.Log("was (re)added to build settings x" + tmp.Count);

                for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    Debug.Log("#" + i + " => " + EditorBuildSettings.scenes[i].path);
                }

                Debug.Log("total build settings scenes x" + EditorBuildSettings.scenes.Length);
            }

        }

        /// <summary>
        /// replace context = remove all other scenes
        /// </summary>
        public void editorLoad(bool replaceContext, bool forceAddBuildSettings = false)
        {
            // first check that scenes are added to build settings ?
            if (forceAddBuildSettings) forceAddToBuildSettings();

            if (verbose)
                Debug.Log($"SceneProfil:editorLoad <b>{label}</b> ; layers x{layers.Count} & deps x{deps.Count}");

            // first : load base scene NON ADDITIVE to replace full context
            // additive check : might wanna replace context
            if (layers.Count > 0)
            {
                UnityEditor.SceneManagement.OpenSceneMode mode = UnityEditor.SceneManagement.OpenSceneMode.Single;
                if (!replaceContext) mode = UnityEditor.SceneManagement.OpenSceneMode.Additive;

                string baseScene = layers[0];
                if (verbose) Debug.Log($"SceneProfil:loading base scene {baseScene}");
                SceneLoaderEditor.loadScene(baseScene, mode);
            }

            List<string> toLoads = new List<string>();

            toLoads.AddRange(layers);
            toLoads.AddRange(deps);
            toLoads.AddRange(statics);

            // load all
            // layers[0] is empty ?
            for (int i = 0; i < toLoads.Count; i++)
            {
                if (verbose) Debug.Log($"SceneProfil:loading layer:{toLoads[i]}");
                SceneLoaderEditor.loadScene(toLoads[i]); // additive
            }
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

            // NOT STATICS : duh

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
                //Debug.LogWarning(getStamp() + " layers array is empty ?");
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

        /// <summary>
        /// 
        /// </summary>
        public void buildUnload(System.Action onUnloadCompleted)
        {

            if (verbose)
                Debug.Log(getStamp() + " build unload : <b>" + label + "</b>");

            if (layers == null)
            {
                if (verbose)
                    Debug.Log(getStamp() + " null layers");

                onUnloadCompleted?.Invoke();
                return;
            }

            if (layers.Count <= 0)
            {
                if (verbose)
                    Debug.Log(getStamp() + " empty layers");

                onUnloadCompleted?.Invoke();
                return;
            }

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

                //if (verbose) Debug.Log("assocs x" + _assocs_buff.Count);
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

        virtual public string stringify()
        {
            string output = label;
            if (!string.IsNullOrEmpty(_profilPath)) output += "     profil path : " + _profilPath;
            if (layers != null) output += "lyr[" + layers.Count + "] & deps[" + deps.Count + "]";
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