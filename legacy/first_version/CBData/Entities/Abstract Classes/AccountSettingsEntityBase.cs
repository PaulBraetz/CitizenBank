using CBData.Abstractions;

using static CBCommon.Enums.CitizenBankEnums;

namespace CBData.Entities
{
    public abstract class AccountSettingsEntityBase : SettingsEntityBase, IAccountSettingsEntity
	{
		protected AccountSettingsEntityBase() { }
		protected AccountSettingsEntityBase(AccountSettingsEntityBase from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			CanBeRecruitedIntoDepartments = from.CanBeRecruitedIntoDepartments;
			ForcePriorityTags = from.ForcePriorityTags;
			TransactionOfferLifetime = from.TransactionOfferLifetime;
			MinimumContractLifeSpan = from.MinimumContractLifeSpan;
			CanReceiveTransactionOffersFor = from.CanReceiveTransactionOffersFor.CloneAsT(circularReferenceHelperDictionary);
			CanCreateTransactionOffersFor = from.CanCreateTransactionOffersFor.CloneAsT(circularReferenceHelperDictionary);
			CanBeMiddlemanFor = from.CanBeMiddlemanFor.CloneAsT(circularReferenceHelperDictionary);
		}
		protected AccountSettingsEntityBase(CurrencyBoolDictionaryEntity canReceiveTransactionOffersFor,
											CurrencyBoolDictionaryEntity canCreateTransactionOffersFor,
											CurrencyBoolDictionaryEntity canBeMiddlemanFor)
		{
			TransactionOfferLifetime = TimeSpan.FromDays((Int32)DefaultTimeSpanDays.Short);
			MinimumContractLifeSpan = TimeSpan.FromDays((Int32)DefaultTimeSpanDays.Long);
			CanReceiveTransactionOffersFor = canReceiveTransactionOffersFor;
			CanCreateTransactionOffersFor = canCreateTransactionOffersFor;
			CanBeMiddlemanFor = canBeMiddlemanFor;
		}

		public virtual Boolean CanBeRecruitedIntoDepartments { get; set; }
		public virtual Boolean ForcePriorityTags { get; set; }
		public virtual TimeSpan TransactionOfferLifetime { get; set; }
		public virtual TimeSpan MinimumContractLifeSpan { get; set; }
		public virtual CurrencyBoolDictionaryEntity CanReceiveTransactionOffersFor { get; set; }
		public virtual CurrencyBoolDictionaryEntity CanCreateTransactionOffersFor { get; set; }
		public virtual CurrencyBoolDictionaryEntity CanBeMiddlemanFor { get; set; }

		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			await Task.WhenAll(
				CanReceiveTransactionOffersFor.SafeEncrypt(encryptor),
				CanCreateTransactionOffersFor.SafeEncrypt(encryptor),
				CanBeMiddlemanFor.SafeEncrypt(encryptor));
			await base.EncryptSelf(encryptor);
		}
		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			await Task.WhenAll(
				CanReceiveTransactionOffersFor.SafeDecrypt(decryptor),
				CanCreateTransactionOffersFor.SafeDecrypt(decryptor),
				CanBeMiddlemanFor.SafeDecrypt(decryptor));
			await base.DecryptSelf(decryptor);
		}
	}
}
