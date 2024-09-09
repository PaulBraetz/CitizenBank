using CBData.Entities;

namespace CBData.Abstractions
{
    public interface IAccountSettingsEntity : ISettingsEntity
	{
		CurrencyBoolDictionaryEntity CanReceiveTransactionOffersFor { get; }
		CurrencyBoolDictionaryEntity CanCreateTransactionOffersFor { get; }
		CurrencyBoolDictionaryEntity CanBeMiddlemanFor { get; }
		Boolean CanBeRecruitedIntoDepartments { get; set; }
		Boolean ForcePriorityTags { get; set; }
		TimeSpan TransactionOfferLifetime { get; set; }
		TimeSpan MinimumContractLifeSpan { get; set; }
	}
}
