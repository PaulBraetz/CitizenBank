
using CBData.Entities;

using PBData.Abstractions;

using System.Collections.Generic;

namespace CBData.Abstractions
{
	public interface IDepartmentEntity : IHasTags,
		IHasPriorityTags,
		IHasAdmins<CitizenEntity>,
		IHasMembers<AccountEntityBase>,
		IHasCreator<CitizenEntity>,
		IHasName
	{
		ICollection<SubDepartmentEntity> SubDepartments { get; set; }
	}
}
