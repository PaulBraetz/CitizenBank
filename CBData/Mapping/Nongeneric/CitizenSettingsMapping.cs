using CBData.Entities;

using PBData.Mapping;

namespace CBData.Mapping
{
	internal class CitizenSettingsMapping : SettingsMappingBase<CitizenSettingsEntity, CitizenEntity>
	{
		public CitizenSettingsMapping()
		{
			Map(m => m.CanBeRecruitedAsDepartmentAdmin);
			Map(m => m.CanBeRecruitedAsAccountAdmin);
		}
	}
}
