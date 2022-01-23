using CBData.Abstractions;

using PBData.Mapping;

namespace CBData.Mapping
{
	internal class DepartmentSettingsMappingBase<TDepartmentSettings, TDepartment> : SettingsMappingBase<TDepartmentSettings, TDepartment>
		where TDepartmentSettings : IDepartmentSettingsEntity<TDepartment>
		where TDepartment : IDepartmentEntity
	{
		public DepartmentSettingsMappingBase()
		{
			Map(m => m.InviteOnly);
		}
	}
}
