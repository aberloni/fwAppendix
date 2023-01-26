using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.industries
{
    /// <summary>
    /// wrapper object to make a factory for a specific type
    /// </summary>
    abstract public class FactoryBase
    {
        //List<FactoryObject> pool = new List<FactoryObject>();
        protected List<iFactoryObject> actives = new List<iFactoryObject>();
        List<iFactoryObject> inactives = new List<iFactoryObject>();

        System.Type _factoryTargetType;

        public FactoryBase()
        {
            _factoryTargetType = getFactoryTargetType();

            IndusReferenceMgr.injectType(_factoryTargetType);

            if (!Application.isPlaying) refresh();
        }

        /// <summary>
        /// what kind of object will be created by this factory
        /// </summary>
        abstract protected System.Type getFactoryTargetType();

        public void refresh()
        {
            Debug.Log(getStamp() + " refresh");

            actives.Clear();
            inactives.Clear();

            //List<T> actives = getActives<T>();
            Object[] presents = (Object[])GameObject.FindObjectsOfType(_factoryTargetType);
            for (int i = 0; i < presents.Length; i++)
            {
                inject(presents[i] as iFactoryObject);
            }

            if (!Application.isPlaying)
            {
                Debug.Log($"[ed] x{actives.Count}");
            }
        }

        //abstract public System.Type getChildrenType();

        public bool hasCandidates() => actives.Count > 0 || inactives.Count > 0;
        public bool hasCandidates(int countCheck) => (actives.Count + inactives.Count) >= countCheck;

        /// <summary>
        /// just transfert list
        /// </summary>
        public List<iFactoryObject> getActives()
        {
            return actives;
        }

        /// <summary>
        /// only for debug
        /// </summary>
        public iFactoryObject[] getInactives()
        {
            return inactives.ToArray();
        }

        public List<T> getActives<T>() where T : iFactoryObject
        {
            List<T> tmp = new List<T>();
            for (int i = 0; i < actives.Count; i++)
            {
                T candid = (T)actives[i];
                if (candid == null) continue;
                tmp.Add(candid);
            }

            //Debug.Log(typeof(T)+" ? candid = "+tmp.Count + " / active count = " + actives.Count);

            return tmp;
        }

        public iFactoryObject getRandomActive()
        {
            Debug.Assert(actives.Count > 0, GetType() + " can't return random one if active list is empty :: " + actives.Count + "/" + inactives.Count);

            return actives[Random.Range(0, actives.Count)];
        }
        public iFactoryObject getNextActive(iFactoryObject curr)
        {
            int idx = actives.IndexOf(curr);
            if (idx > -1)
            {
                if (idx + 1 < actives.Count) return actives[idx + 1];
                return actives[0]; // loop
            }

            Debug.LogError(curr + " is not in factory ?");

            return null;
        }

        /// <summary>
        /// générer un nouveau element dans le pool
        /// </summary>
        protected iFactoryObject create(string subType)
        {
            string path = System.IO.Path.Combine(getObjectPath(), subType);
            Object obj = Resources.Load(path);
            Debug.Assert(obj != null, $"{GetType()}&{_factoryTargetType} no object to load at path : " + path);

            obj = GameObject.Instantiate(obj);
            Debug.Log(getStamp() + " created:" + obj, obj);

            GameObject go = obj as GameObject;

            //Debug.Log("newly created object " + go.name, go);

            iFactoryObject candidate = go.GetComponent<iFactoryObject>();
            Debug.Assert(candidate != null, $"no candidate on {go} ?? generated object is not factory compatible", go);

            inactives.Add(candidate);
            //recycle(candidate);

            //for refs list
            //IndusReferenceMgr.refreshGroupByType(factoryTargetType);
            //IndusReferenceMgr.injectObject(candidate);

            return candidate;
        }
        abstract protected string getObjectPath();

        /// <summary>
        /// demander a la factory de filer un element dispo
        /// subType est le nom du prefab dans le dossier correspondant
        /// </summary>
        public iFactoryObject extract(string subType)
        {
            iFactoryObject obj = null;

            //will add an item in inactive
            //and go on
            if (inactives.Count > 0)
            {

                // search in available pool
                for (int i = 0; i < inactives.Count; i++)
                {
                    if (inactives[i].factoGetCandidateName() == subType)
                    {
                        obj = inactives[i];
                    }
                }

            }

            // none available, create a new one
            if (obj == null)
            {
                Debug.Log(getStamp() + " no " + subType + " available (x" + inactives.Count + ") creating one");
                obj = create(subType);
            }

            // make it active
            inject(obj);

            //va se faire tout seul au setup()
            //obj.materialize();

            return obj;
        }

        public T extract<T>(string subType)
        {
            iFactoryObject icand = extract(subType);
            Component com = icand as Component;
            return com.GetComponent<T>();
        }

        /// <summary>
        /// indiquer a la factory qu'un objet a changé d'état de recyclage
        /// </summary>
        public void recycle(iFactoryObject candid)
        {
            bool present = actives.IndexOf(candid) >= 0;
            //Debug.Assert(present, candid + " is not in actives array ?");
            if (present)
            {
                actives.Remove(candid);
            }

            present = inactives.IndexOf(candid) >= 0;
            //Debug.Assert(!present, candid + " must not be already in inactives");
            if (!present)
            {
                inactives.Add(candid);

                candid.factoRecycle();

                IndusReferenceMgr.removeObject(candid); // rem facebook
            }

            // move recycled object into facto scene
            MonoBehaviour comp = candid as MonoBehaviour;
            if (comp != null)
            {
                comp.transform.SetParent(null);

                // do something more ?
                //comp.gameObject.SetActive(false);
                //comp.enabled = false;

                /*
                //https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.MoveGameObjectToScene.html
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(
                    comp.gameObject,
                    UnityEngine.SceneManagement.SceneManager.GetSceneByName(TinyConst.scene_resources_facto));
                */
            }

            log(" :: recycle :: " + candid + " :: ↑" + actives.Count + "/ ↓" + inactives.Count);
        }

        /// <summary>
        /// quand un objet est déclaré comme utilisé par le systeme
        /// généralement cette méthode est appellé a la création d'un objet lié a la facto
        /// </summary>
        public void inject(iFactoryObject candid)
        {
            inactives.Remove(candid);

            if (actives.IndexOf(candid) < 0)
            {
                actives.Add(candid);

                log(" :: inject :: " + candid + " :: ↑" + actives.Count + "/ ↓" + inactives.Count);

                candid.factoMaterialize();

                MonoBehaviour cmp = candid as MonoBehaviour;
                if (cmp != null) cmp.enabled = true;

                IndusReferenceMgr.injectObject(candid);
            }

        }

        /// <summary>
        /// called by a destroyed object
        /// </summary>
        public void destroy(iFactoryObject candid)
        {
            if (actives.IndexOf(candid) > -1) actives.Remove(candid);
            if (inactives.IndexOf(candid) > -1) inactives.Remove(candid);
        }

        public void recycleAll()
        {
            Debug.Log(getStamp() + " recycleAll");

            List<iFactoryObject> cands = new List<iFactoryObject>();
            cands.AddRange(actives);

            for (int i = 0; i < cands.Count; i++)
            {
                recycle(cands[i]);
            }

            Debug.Assert(actives.Count <= 0);
        }


        /// <summary>
        /// https://docs.microsoft.com/fr-fr/dotnet/api/system.type.isassignablefrom?view=net-5.0
        /// </summary>
        public bool isFactoSaveLinked()
        {
            //bool assign = factoryTargetType.IsAssignableFrom(typeof(ISaveSerializable));
            bool assign = typeof(iSaveSerializable).IsAssignableFrom(getFactoryTargetType());
            //Debug.Log(factoryTargetType + " => " + assign);
            return assign;

            /*
            if (!hasCandidates()) return false;

            IFactoryObject obj = null;
            if (actives.Count > 0) obj = actives[0];
            else if (inactives.Count > 0) obj = inactives[0];
            else Debug.LogError("nope");

            //bool assign = actives[0].GetType().IsAssignableFrom(typeof(ISaveSerializable));
            bool assign = (obj as ISaveSerializable) != null;
            Debug.Log(actives[0].GetType() + " vs " + typeof(ISaveSerializable)+" => "+assign);

            return assign;
            */
        }
    
        string getStamp() => "<color=#3333aa>" + GetType() + "</color>";

        void log(string content)
        {
#if UNITY_EDITOR || industries
            Debug.Log(getStamp() + content);
#endif
        }
    }

    //public interface IFactory{}

    public interface iFactoryObject : iIndusReference, iSaveSerializable
    {

        /// <summary>
        /// the actual name of the object to instantiate
        /// Resources/{facto}/{CandidateName}
        /// </summary>
        string factoGetCandidateName();

        /// <summary>
        /// when object is added to factory lists
        /// this is called when object is recycled
        /// describe recycling process
        /// +must tell factory
        /// </summary>
        void factoRecycle();

        /// <summary>
        /// when object is added to factory lists
        /// this is called when factory provide this object
        /// describe activation
        /// called when added to actives
        /// </summary>
        void factoMaterialize();

        //string serialize();
    }

    public interface iSaveSerializable
    {
        object generateSerialData(); // generate an objet to be saved

        void mergeSerialData(object data); // use a deserialized object to get data

        //void mergeId(TinyIdentity datId);
    }

}
