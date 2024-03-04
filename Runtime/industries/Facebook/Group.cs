using System.Collections.Generic;
using System.Collections.ObjectModel;

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
	}
}
