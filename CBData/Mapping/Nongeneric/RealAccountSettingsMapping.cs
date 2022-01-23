using CBData.Entities;

namespace CBData.Mapping
{
	internal class RealAccountSettingsMapping : AccountSettingsMappingBase<RealAccountSettingsEntity>
	{
		public RealAccountSettingsMapping()
		{
			References(m => m.CanBeDepositAccountFor).Cascade.All();
		}
	}
}
