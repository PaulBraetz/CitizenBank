using CBData.Abstractions;

using PBData.Mapping;

namespace CBData.Mapping
{
	internal class AccountSettingsMappingBase<TAccountSettingsEntity> : SettingsMappingBase<TAccountSettingsEntity>
		where TAccountSettingsEntity : IAccountSettingsEntity
	{
		public AccountSettingsMappingBase()
		{
			Map(m => m.CanBeRecruitedIntoDepartments);
			Map(m => m.ForcePriorityTags);
			Map(m => m.TransactionOfferLifetime);
			Map(m => m.MinimumContractLifeSpan);
			References(m => m.CanReceiveTransactionOffersFor).Cascade.All();
			References(m => m.CanCreateTransactionOffersFor).Cascade.All();
			References(m => m.CanBeMiddlemanFor).Cascade.All();
		}
	}
}
