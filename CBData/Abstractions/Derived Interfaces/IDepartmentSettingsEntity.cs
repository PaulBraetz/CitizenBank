
using PBData.Abstractions;
using System;

namespace CBData.Abstractions
{
	public interface IDepartmentSettingsEntity<TDepartment> : ISettingsEntity<TDepartment>
		where TDepartment : IDepartmentEntity
	{
		public Boolean InviteOnly { get; set; }
	}
}
