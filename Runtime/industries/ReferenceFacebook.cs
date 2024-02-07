using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//using Object = UnityEngine.Object;
using System.Linq;

namespace fwp.industries
{

    /// <summary>
    /// manage a dictionary of assoc between a given Type and all objects associated with it
    /// </summary>
    abstract public class ReferenceFacebook<FaceType> where FaceType : class
    {
        public bool verbose = false;

        private Dictionary<Type, List<FaceType>> candidates;

        public ReferenceFacebook()
        {
            candidates = new Dictionary<Type, List<FaceType>>();
        }

        /// <summary>
        /// refresh all existing
        /// </summary>
        public void refreshAll()
        {
            MonoBehaviour[] monos = GameObject.FindObjectsOfType<MonoBehaviour>();

            log("refreshAll() checking x" + candidates.Count + " types against x" + monos.Length + " monos");

            foreach (var kp in candidates)
            {
                kp.Value.Clear();
                kp.Value.AddRange(fetchByType(kp.Key, monos));
            }
        }

        private Type getTypeByDicoIndex(int idx)
        {
            int i = 0;
            foreach (var kp in candidates)
            {
                if (i == idx) return kp.Key;
            }
            return null;
        }

        public bool hasAnyType() => candidates.Count > 0;

        public Type[] getAllTypes()
        {
            List<Type> output = new List<Type>();
            foreach (var kp in candidates)
            {
                output.Add(kp.Key);
            }
            return output.ToArray();
        }

        private bool hasGroupOfType(Type tar)
        {
            foreach (var kp in candidates)
            {
                //Debug.Log(kp.Key + " vs " + tar);
                //if (kp.Key.GetType().IsAssignableFrom(tar)) return true;
                if (tar.IsAssignableFrom(kp.Key)) return true;
            }
            return false;
        }

        private bool hasGroupType<T>()
        {
            foreach (var kp in candidates)
            {
                if (typeof(T).IsAssignableFrom(kp.Key)) return true;
                //if (kp.Key.GetType() == typeof(T)) return true;
            }
            return false;
        }

        /// <summary>
        /// generate a list of candidates, NOT using facebook
        /// only fetching objects
        /// NOT OPTI if no monos are provided
        /// </summary>
        private List<FaceType> fetchByType(Type tar, MonoBehaviour[] monos = null)
        {
            List<FaceType> output = new List<FaceType>();

            //gather group data
            if (monos == null) monos = GameObject.FindObjectsOfType<MonoBehaviour>();

            for (int i = 0; i < monos.Length; i++)
            {
                //Debug.Log(typ + " vs " + monos[i].GetType());

                FaceType iref = monos[i] as FaceType;
                if (iref == null) continue;

                //if (monos[i].GetType().IsAssignableFrom(tar))
                if (tar.IsAssignableFrom(iref.GetType()))
                {
                    output.Add(iref);
                }
            }

            return output;
        }

        /// <summary>
        /// get all mono and inject all object of given type into facebook
        /// </summary>
        public List<FaceType> refreshGroupByType(Type tar, MonoBehaviour[] monos = null)
        {
            var output = fetchByType(tar, monos);
            candidates[tar] = output;

            log($"group refresh <{tar}> x" + output.Count);

            return output;
        }

        /// <summary>
        /// faaat at runtime
        /// </summary>
        public List<T> refreshGroup<T>(MonoBehaviour[] monos = null) where T : FaceType
        {
            List<T> output = new List<T>();

            var list = refreshGroupByType(typeof(T), monos);
            for (int i = 0; i < list.Count; i++)
            {
                var cand = (T)list[i];
                output.Add(cand);
            }

            return output;
        }

        /// <summary>
        /// auto file object in matching category
        /// </summary>
        /// <param name="target"></param>
        public void injectObject(FaceType target)
        {
            if(target == null)
            {
                Debug.LogError("null object given, must prevent sub to facebook for null objects");
                Debug.LogError("object might not be compatible with this FaceType ? "+typeof(FaceType));
                return;
            }
            
            injectObject(target, target.GetType());
        }

        /// <summary>
        /// meant to specify what category to store the object
        /// </summary>
        public void injectObject<T>(FaceType target) where T : FaceType => injectObject(target, typeof(T));

        /// <summary>
        /// if type is not declared facebook will add it AND fetch
        /// </summary>
        public void injectObject(FaceType target, Type targetType)
        {
            Debug.Assert(target != null, "do not inject null object ?");

            if (!hasAssocType(targetType))
            {
                // this will also fetch all of this type
                // it seems that findobjectoftype can't find target during awake
                injectType(targetType);
            }

            if (candidates[targetType].IndexOf(target) < 0) // already subbed ?
            {
                candidates[targetType].Add(target);

                log("inject :: type:" + targetType + " & ref : " + target + " :: ↑" + candidates[targetType].Count);
            }

        }

        /// <summary>
        /// should remove object in ALL list where it's located
        /// </summary>
        public void removeObject(FaceType target)
        {

            var list = getAssocTypes(target);
            if (list.Count <= 0)
            {
                if (verbose)
                    Debug.LogWarning("trying to remove object " + target + " by no assoc type found ?");

                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                var targetType = list[i];

                candidates[targetType].Remove(target);

                log("recycle :: type:" + targetType + " & ref : " + target + " :: ↑" + candidates[targetType].Count);
            }
        }

        /// <summary>
        /// assignable definition : https://www.geeksforgeeks.org/c-sharp-type-isassignablefromtype-method/
        /// </summary>
        Type getAssocType(FaceType target) => getAssocType(target.GetType());

        Type getAssocType(Type tar)
        {
            // must search for compatible type, NOT the type of target
            // some targets are from diff types BUT have a parent common type for indus
            foreach (var kp in candidates)
            {
                //bool ass = tar.IsAssignableFrom(kp.Key);
                //Debug.Log(tar + " assignable " + kp.Key + " ? " + ass);
                bool ass = kp.Key.IsAssignableFrom(tar);
                //Debug.Log(kp.Key + " assignable " + tar + " ? " + ass);

                //if (kp.Key.GetType().IsAssignableFrom(tar)) return true;
                if (ass)
                {
                    return kp.Key;
                }
            }

            return null;
        }

        List<Type> getAssocTypes(FaceType target) => getAssocTypes(target.GetType());
        List<Type> getAssocTypes(Type tar)
        {
            List<Type> output = new List<Type>();
            foreach (var kp in candidates)
            {
                bool ass = kp.Key.IsAssignableFrom(tar);
                if (ass)
                {
                    output.Add(kp.Key);
                }
            }

            return output;
        }

        bool hasAssocType(Type tar)
        {
            return candidates.ContainsKey(tar);
        }

        /// <summary>
        /// add a specific type and its solved list to facebook
        /// if type is not declared in facebook, it will add it AND fetch
        /// </summary>
        public void injectType(Type tar)
        {
            //var assoc = getAssocType(tar);
            if (hasAssocType(tar)) return;

            candidates.Add(tar, new List<FaceType>());

            log($"{getStamp()} facebook added type : <b>{tar}</b>");

            fetchByType(tar);

            log($"{getStamp()} found x{candidates[tar].Count} ref(s) after adding type : <b>{tar}</b>");
        }

        public void injectTypes(Type[] tars)
        {
            for (int i = 0; i < tars.Length; i++)
            {
                injectType(tars[i]);
            }
        }

        public List<FaceType> getGroupByType(Type tar)
        {
            List<FaceType> output = new List<FaceType>();
            foreach (var kp in candidates)
            {
                if (tar == kp.Key) return kp.Value;
            }
            return output;
        }

        public List<T> getGroup<T>() where T : FaceType
        {
            // check in facebook if it has the group
            // by checking assignable type (not absolute type)
            if (!hasGroupType<T>())
            {
                if (verbose)
                    Debug.LogWarning("no group " + typeof(T) + " ?");

                return null;
            }

            //Type assoc = getAssocType(typeof(T));
            Type assoc = typeof(T);
            List<FaceType> elmts = candidates[assoc];
            Debug.Assert(elmts != null, "facebook list not init for type " + assoc);

            // /! using Linq here
            List<T> output = elmts.Cast<T>().ToList();
            Debug.Assert(output != null, "can't cast " + elmts + " to " + assoc);

            return output;
        }

        public MonoBehaviour getClosestToPosition(Type tar, Vector2 position)
        {
            List<FaceType> refs = getGroupByType(tar);
            FaceType closest = null;
            float min = Mathf.Infinity;
            float dst;

            for (int i = 0; i < refs.Count; i++)
            {
                MonoBehaviour mono = refs[i] as MonoBehaviour;
                if (mono == null) continue;

                dst = Vector2.Distance(mono.transform.position, position);

                if (dst < min)
                {
                    min = dst;
                    closest = mono as FaceType;
                }
            }

            return closest as MonoBehaviour;
        }

        string getStamp() => "<color=#3333aa>~" + typeof(FaceType).ToString() + "</color>";

        public void log(string content)
        {

#if UNITY_EDITOR || industries
            bool showLog = verbose;

            if (showLog)
                Debug.Log(getStamp() + content);
#endif
        }

    }

}