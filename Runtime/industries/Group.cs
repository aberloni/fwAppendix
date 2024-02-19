using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TinyEngine.Collections
{
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
			T tMember = member as T;
			if (tMember != null && !members.Contains(tMember))
				members.Add(tMember);
		}

		public void Remove(object member)
		{
			T tMember = member as T;
			if (tMember != null && members.Contains(tMember))
				members.Remove(tMember);
		}
	}
}
