using CBData.Entities;

using PBData.Abstractions;

using System.Collections.Generic;

namespace CBData.Abstractions
{
	public interface IHasPriorityTags : IEntity
	{
		ICollection<TagEntity> PriorityTags { get; set; }
	}
}
