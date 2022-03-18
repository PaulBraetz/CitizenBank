using CBData.Entities;

namespace CBData.Mapping
{
	internal class VirtualAccountSettingsMapping : AccountSettingsMappingBase<VirtualAccountSettingsEntity>
	{
		public VirtualAccountSettingsMapping()
		{
			Map(m => m.DepositForwardLifeSpan);
			Map(m => m.DefaultDepositAccountMapRelativeLimit);
			Map(m => m.DefaultDepositAccountMapAbsoluteLimit);
		}
	}
}
