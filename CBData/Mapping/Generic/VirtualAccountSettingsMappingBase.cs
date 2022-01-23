using CBData.Abstractions;

namespace CBData.Mapping
{
	internal class VirtualAccountSettingsMappingBase<TSettings> : AccountSettingsMappingBase<TSettings>
		where TSettings : IVirtualAccountSettingsEntity
	{
		public VirtualAccountSettingsMappingBase()
		{
			Map(m => m.DepositForwardLifeSpan);
			Map(m => m.DefaultDepositAccountMapRelativeLimit);
			Map(m => m.DefaultDepositAccountMapAbsoluteLimit);
		}
	}
}
