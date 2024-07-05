using CBData.Entities;

using PBData.Abstractions;

using System.Collections.Generic;

namespace CBData.Abstractions
{
	public interface IHasTags : IEntity
	{
		ICollection<TagEntity> Tags { get; set; }
	}
}
