using CBData.Abstractions;

using PBData.Mapping;

namespace CBData.Mapping
{
	internal class DepartmentSettingsMappingBase<TDepartmentSettings> : SettingsMappingBase<TDepartmentSettings>
		where TDepartmentSettings : IDepartmentSettingsEntity
	{
		public DepartmentSettingsMappingBase()
		{
			Map(m => m.InviteOnly);
		}
	}
}
