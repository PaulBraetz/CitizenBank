
using PBData.Abstractions;

using System.Collections.Generic;

namespace CBData.Abstractions
{
	public interface IHasMembers<TMember> : IEntity
		where TMember : IEntity
	{
		ICollection<TMember> Members { get; set; }
	}
}
