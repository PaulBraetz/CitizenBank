using CBData.Entities;

namespace CBData.Abstractions
{
    public interface IHasTags : IEntity
	{
		ICollection<TagEntity> Tags { get; set; }
	}
}
