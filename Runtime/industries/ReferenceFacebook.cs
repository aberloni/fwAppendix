using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;

//using Object = UnityEngine.Object;
using System.Linq;

namespace fwp.industries
{

    /// <summary>
    /// manage a dictionary of assoc between a given Type and all objects associated with it
    /// in : BaseType can be an interface shared by all candidates
    /// </summary>
    abstract public class ReferenceFacebook<FaceType> where FaceType : class
    {
        public bool verbose = false;

        private Dictionary<Type, List<FaceType>> candidates;
        private Dictionary<Type, ReadOnlyCollection<FaceType>> collections;

        private Dictionary<Type, Group<FaceType>> groups;
        

        public class Group<T> where T : class
        {
            public List<T> candidates;
            public ReadOnlyCollection<T> collections;
        }

        public ReferenceFacebook()
        {
            candidates = new Dictionary<Type, List<FaceType>>();
            collections = new Dictionary<Type, ReadOnlyCollection<FaceType>>();

            groups = new Dictionary<Type, Group<FaceType>>();
        }

        public bool hasAnyType() => candidates.Count > 0;

        /// <summary>
        /// refresh all existing
        /// </summary>
        public void refreshAll()
        {
            MonoBehaviour[] monos = GameObject.FindObjectsOfType<MonoBehaviour>();

            log("refreshAll() checking x" + candidates.Count + " types against x" + monos.Length + " monos");

            foreach (var kp in candidates)
            {
                refreshType(kp.Key, monos);
            }
        }

        public void refreshType<T>(MonoBehaviour[] monos = null) where T : FaceType
            => refreshType(typeof(T), monos);

        public void refreshType(Type type, MonoBehaviour[] monos = null)
        {
            if (!ContainsType(type))
                return;

            var list = candidates[type];
            list.Clear();

            //gather group data
            if (monos == null) monos = GameObject.FindObjectsOfType<MonoBehaviour>();

            for (int i = 0; i < monos.Length; i++)
            {
                //Debug.Log(typ + " vs " + monos[i].GetType());

                FaceType iref = monos[i] as FaceType;
                if (iref == null) continue;

                //if (monos[i].GetType().IsAssignableFrom(tar))
                if (type.IsAssignableFrom(iref.GetType()))
                {
                    list.Add(iref);
                }
            }

        }
        public Type[] getAllTypes()
        {
            List<Type> output = new List<Type>();
            foreach (var kp in candidates)
            {
                output.Add(kp.Key);
            }
            return output.ToArray();
        }

        public bool ContainsType<T>() => ContainsType(typeof(T));
        public bool ContainsType(Type t) => candidates.ContainsKey(t);

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
        public void injectObject<T>(T target) where T : FaceType 
            => injectObject(target, typeof(T));

        /// <summary>
        /// if type is not declared facebook will add it AND fetch
        /// </summary>
        public void injectObject(FaceType target, Type targetType)
        {
            Debug.Assert(target != null, "do not inject null object ?");

            if(!ContainsType(targetType))
            {
                // this will also fetch all of this type
                // it seems that findobjectoftype can't find target during awake
                injectType(targetType);
            }

            var list = candidates[targetType];
            if(!list.Contains(target))
            {
                list.Add(target);
                log("inject :: type:" + targetType + " & ref : " + target + " :: ↑" + candidates[targetType].Count);
            }
        }

        /// <summary>
        /// should remove object in ALL list where it's located
        /// will also remove all assoc types
        /// </summary>
        public void removeObject(FaceType target)
        {
            var compatList = getAssocTypes(target);

            if (compatList.Count <= 0)
            {
                if (verbose)
                    Debug.LogWarning("trying to remove object " + target + " by no assoc type found ?");

                return;
            }

            for (int i = 0; i < compatList.Count; i++)
            {
                var targetType = compatList[i];

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
        public void injectType(Type tar, bool alsoFetch = false)
        {
            //var assoc = getAssocType(tar);
            if (hasAssocType(tar)) return;

            var list = new List<FaceType>();
            candidates.Add(tar, list);

            //var ro = new ReadOnlyCollection<FaceType>(candidates[tar]);
            var ro = list.AsReadOnly();
            collections.Add(tar, ro);

            log($"{getStamp()} facebook added type : <b>{tar}</b>");

            if(alsoFetch)
            {
                refreshType(tar);
                log($"{getStamp()} found x{candidates[tar].Count} ref(s) after adding type : <b>{tar}</b>");
            }
        }

        public void injectTypes(Type[] tars)
        {
            for (int i = 0; i < tars.Length; i++)
            {
                injectType(tars[i]);
            }
        }

        public List<T> getCopy<T>() where T : FaceType
        {
            if (!ContainsType<T>()) return null;
            return candidates[typeof(T)].Cast<T>().ToList();
        }

        /// <summary>
        /// in  : <T>
        /// out : list of objects of that type
        /// </summary>
        public ReadOnlyCollection<T> getCollection<T>() where T : FaceType
        {
            if (!ContainsType<T>()) return null;

            //var g = groups[typeof(T)];
            //return g.collections;

            //creates copy
            var list = candidates[typeof(T)].Cast<T>().ToList();
            return new ReadOnlyCollection<T>(list);

            //var collec = (ReadOnlyCollection<T>)collections[typeof(T)];
            //return collec;

            //return candidates[typeof(T)].AsReadOnly();

            //return null;

        }

        public ReadOnlyCollection<FaceType> getCollection(Type t)
        {
            if (!ContainsType(t)) return null;
            //var g = groups[t];
            //return g.collections;
            //return new ReadOnlyCollection<FaceType>(candidates[t]);
            return collections[t];
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