using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>
/// author : Lorris Giovagnoli
/// </summary>
namespace fwp.industries.facebook
{
	public interface IGroup
	{
		void Add(object member);

		void Remove(object member);

		void Clear();

		IReadOnlyList<object> GetMembers();
	}

	public class Group<T> : IGroup where T: class
	{
		public List<T> members { get; }
		public ReadOnlyCollection<T> readOnlyMembers { get; }

		public Group() : this(new List<T>()) { }

		public Group(List<T> members)
		{
			this.members = members;
			readOnlyMembers = new ReadOnlyCollection<T>(members);
		}

		public void Add(object member)
		{
			//compat check
			T elmt = member as T;
			if (elmt == null)
				return;

			if (member is T typedMember
				&& !members.Contains(typedMember))
			{
				members.Add(typedMember);
			}
		}

		public void Remove(object member)
		{
			if (member is T typedMember
				&& members.Contains(typedMember))
			{
				members.Remove(typedMember);
			}
		}

		public IReadOnlyList<object> GetMembers()
		{
			return readOnlyMembers;
		}

		public void Clear()
        {
			members.Clear();
        }

		public bool IsSubclass(System.Type typ)
        {
			// https://learn.microsoft.com/en-us/dotnet/api/system.type.issubclassof?view=net-8.0
			// The IsSubclassOf method cannot be used to determine whether an interface derives from another interface,
			// or whether a class implements an interface.
			// Use the IsAssignableFrom method for that purpose, as the following example shows.

			return typ.IsSubclassOf(typeof(T));
		}

		public bool IsAssignableFrom(System.Type typ)
        {
			// https://learn.microsoft.com/en-us/dotnet/api/system.type.isassignablefrom?view=net-8.0
			// typ castable as T ?
			//return typeof(T).IsAssignableFrom(typ);
			return typ.IsAssignableFrom(typeof(T));
        }
	}
}
