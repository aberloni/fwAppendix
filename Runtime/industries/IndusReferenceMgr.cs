using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// FACEBOOK wrapper
/// need to specify compatible types
/// </summary>
static public class IndusReferenceMgr
{
    /// <summary>
    /// it knows EVERYBODY
    /// </summary>
    static private Dictionary<Type, List<iIndusReference>> facebook = new Dictionary<Type, List<iIndusReference>>();

    static public void edRefresh()
    {
        Debug.LogWarning("editor refresh of facebook");

        refreshAll();
    }

    /// <summary>
    /// refresh all existing
    /// </summary>
    static public void refreshAll()
    {
        MonoBehaviour[] monos = GameObject.FindObjectsOfType<MonoBehaviour>();

        Debug.Log(getStamp() + " checking x"+facebook.Count+" types against x" + monos.Length + " monos");

        foreach (var kp in facebook)
        {
            kp.Value.Clear();
            kp.Value.AddRange(fetchByType(kp.Key, monos));
        }
    }

    static private Type getTypeByDicoIndex(int idx)
    {
        int i = 0;
        foreach (var kp in facebook)
        {
            if (i == idx) return kp.Key;
        }
        return null;
    }

    static public bool hasAnyType() => facebook.Count > 0;

    static public Type[] getAllTypes()
    {
        List<Type> output = new List<Type>();
        foreach (var kp in facebook)
        {
            output.Add(kp.Key);
        }
        return output.ToArray();
    }

    static private bool hasGroupOfType(Type tar)
    {
        foreach (var kp in facebook)
        {
            //Debug.Log(kp.Key + " vs " + tar);
            //if (kp.Key.GetType().IsAssignableFrom(tar)) return true;
            if (tar.IsAssignableFrom(kp.Key)) return true;
        }
        return false;
    }

    static private bool hasGroupType<T>()
    {
        foreach (var kp in facebook)
        {
            if (typeof(T).IsAssignableFrom(kp.Key)) return true;
            //if (kp.Key.GetType() == typeof(T)) return true;
        }
        return false;
    }

    static public List<iIndusReference> fetchByType(Type tar, MonoBehaviour[] monos = null)
    {
        List<iIndusReference> output = new List<iIndusReference>();

        //gather group data
        if (monos == null) monos = GameObject.FindObjectsOfType<MonoBehaviour>();

        for (int i = 0; i < monos.Length; i++)
        {
            //Debug.Log(typ + " vs " + monos[i].GetType());

            iIndusReference iref = monos[i] as iIndusReference;
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
    static public List<iIndusReference> refreshGroupByType(Type tar, MonoBehaviour[] monos = null)
    {
        var output = fetchByType(tar, monos);
        facebook[tar] = output;

        Debug.Log($"{getStamp()} group refresh <{tar}> x"+output.Count);

        return output;
    }

    /// <summary>
    /// faaat at runtime
    /// </summary>
    static public List<T> refreshGroup<T>(MonoBehaviour[] monos = null) where T : iIndusReference
    {
        List<iIndusReference> iir = refreshGroupByType(typeof(T), monos);

        List<T> output = iir as List<T>;
        Debug.Assert(output != null);

        return output;
    }

    static public void injectObject(iIndusReference target)
    {
        Debug.Assert(target != null);

        Type tar = getAssocType(target);

        if(tar == null)
        {
            Debug.LogWarning(getStamp() + " no assoc type for target " + target+" , can't inject");
        }
        else if (facebook[tar].IndexOf(target) < 0) // already subbed ?
        {
            facebook[tar].Add(target);
        }
        
    }

    static public void removeObject(iIndusReference target)
    {
        var assoc = getAssocType(target);

        facebook[assoc].Remove(target);

        Debug.Log("removed " + target+" from "+assoc+" x"+facebook[assoc].Count);
    }

    /// <summary>
    /// assignable definition : https://www.geeksforgeeks.org/c-sharp-type-isassignablefromtype-method/
    /// </summary>
    static Type getAssocType(iIndusReference target)
    {
        Type tar = target.GetType();
        return getAssocType(tar);
    }

    static Type getAssocType(Type tar)
    {
        // must search for compatible type, NOT the type of target
        // some targets are from diff types BUT have a parent common type for indus
        foreach (var kp in facebook)
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

    /// <summary>
    /// add a specific type and its solved list to facebook
    /// </summary>
    static public void injectType(Type tar)
    {
        var assoc = getAssocType(tar);
        if (assoc == null)
        {
            Debug.Log($"{getStamp()} adding new group to facebook : <b>{tar}</b>");
            facebook.Add(tar, new List<iIndusReference>());
        }
    }

    static public List<iIndusReference> getGroupByType(Type tar)
    {
        List<iIndusReference> output = new List<iIndusReference>();
        foreach (var kp in facebook)
        {
            if (tar == kp.Key) return kp.Value;
        }
        return output;
    }

    static public List<T> getGroup<T>() where T : iIndusReference
    {
        if(!hasGroupType<T>())
        {
            Debug.LogWarning("no group " + typeof(T) + " ?");
            return null;
        }

        //Type assoc = getAssocType(typeof(T));
        Type assoc = typeof(T);
        List<iIndusReference> elmts = facebook[assoc];
        Debug.Assert(elmts != null, "facebook list not init for type " + assoc);

        List<T> output = elmts.Cast<T>().ToList();
        Debug.Assert(output != null, "can't cast " + elmts + " to " + assoc);

        return output;
    }

    static public MonoBehaviour getClosestToPosition(Type tar, Vector2 position)
    {
        List<iIndusReference> refs = getGroupByType(tar);
        iIndusReference closest = null;
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
                closest = mono as iIndusReference;
            }
        }

        return closest as MonoBehaviour;
    }

    static string getStamp() => "~indus:";
}

public interface iIndusReference
{

}