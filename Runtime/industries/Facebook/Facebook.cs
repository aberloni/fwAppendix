using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

/// <summary>
/// author : Lorris Giovagnoli
/// </summary>
namespace fwp.industries.facebook
{
    /// <summary>
    /// compatible objects need to implem this interface
    /// </summary>
    public interface IFacebook { }

    /// <summary>
    /// meant to provide a way to manage list of type of objects
    /// where context objects can sub/unsub from
    /// 
    /// to dodge having to manage a FindObjectsOfType anywhere
    /// </summary>
    abstract public class Facebook
    {
        private Dictionary<Type, IGroup> registry = new Dictionary<Type, IGroup>();

#if UNITY_EDITOR
        /// <summary>
        /// debug only
        /// </summary>
        public List<Type> GetAllTypes()
        {
            List<Type> typs = new List<Type>();
            foreach (var kp in registry)
            {
                typs.Add(kp.Key);
            }
            return typs;
        }

        /// <summary>
        /// !OPTI
        /// will refill array based on monobehavior found in context
        /// </summary>
        void Refresh(IGroup group, MonoBehaviour[] monos = null)
        {
            if (monos == null) monos = fwp.appendix.AppendixUtils.gcs<MonoBehaviour>();

			group.Clear();

			foreach (var m in monos)
            {
                group.Add(m);
            }
        }

        public void Refresh(System.Type groupType, MonoBehaviour[] monos = null)
        {
			if (!registry.ContainsKey(groupType)) return;
			Refresh(registry[groupType], monos);
		}
        public void Refresh<IGroup>(MonoBehaviour[] monos = null) => Refresh(typeof(IGroup), monos);
        
        public void RefreshAll(MonoBehaviour[] monos = null)
        {
			if (monos == null) monos = fwp.appendix.AppendixUtils.gcs<MonoBehaviour>();

            foreach (var r in registry)
            {
                Refresh(r.Value, monos);
            }
        }
#endif

        public void Register<T>(T member) where T : class, IFacebook
        {
            if (member == null)
                return;

            Group<T> group = GetGroupOfType<T>();
            if (!group.members.Contains(member))
                group.members.Add(member);
        }

        /// <summary>
        /// only removed from matching highest level Type
        /// </summary>
        public void Delete<T>(T member) where T : class, IFacebook
        {
            if (member == null)
                return;

            Group<T> group = GetGroupOfType<T>();
            if (group.members.Contains(member))
            {
                group.members.Remove(member);
            }
        }

        /// <summary>
        /// its type and all castables
        /// </summary>
        public void DeleteFromAllGroups(object member)
        {
            if (member == null)
                return;

            //Debug.Log("deleting : " + member);

            Type memberType = member.GetType();
            foreach (Type type in registry.Keys)
            {
                //Debug.Log("? " + type + " vs " + memberType);
                if (type.IsAssignableFrom(memberType))
                {
                    //Debug.Log("!");
                    registry[type].Remove(member);
                }
            }
        }

        /// <summary>
        /// returns a copy of content of a group
        /// </summary>
        public List<T> GetGroupCopy<T>() where T : class, IFacebook
        {
            List<T> output = new List<T>();
            output.AddRange(GetGroup<T>());
            return output;
        }

        public ReadOnlyCollection<T> GetGroup<T>() where T : class, IFacebook
        {
            Group<T> group = GetGroupOfType<T>();
            return group.readOnlyMembers;
        }

        private Group<T> GetGroupOfType<T>() where T : class, IFacebook
        {
            CreateGroupOfTypeIfDoesNotExist<T>();
            return (Group<T>)registry[typeof(T)];
        }

        private void CreateGroupOfTypeIfDoesNotExist<T>() where T : class, IFacebook
        {
            if (registry.ContainsKey(typeof(T)))
                return;

            Group<T> newGroup = new Group<T>();
            registry.Add(typeof(T), newGroup);
        }

        public IReadOnlyList<object> GetGroup(Type type)
        {
            return GetGroupOfType(type)?.GetMembers();
        }

        private IGroup GetGroupOfType(Type type)
        {
            return registry.ContainsKey(type) ? registry[type] : null;
        }
    }
}
