
using CBData.Entities;

namespace CBData.Abstractions
{
    public interface IDepartmentEntity : IHasTags,
		IHasPriorityTags,
		IHasCreator<CitizenEntity>,
		IHasName
	{
		ICollection<SubDepartmentEntity> SubDepartments { get; set; }
	}
}
