using CBData.Entities;

namespace CBData.Abstractions
{
    public interface IHasPriorityTags : IEntity
	{
		ICollection<TagEntity> PriorityTags { get; set; }
	}
}
