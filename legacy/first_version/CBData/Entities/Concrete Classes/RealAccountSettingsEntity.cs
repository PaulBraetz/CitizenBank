namespace CBData.Entities
{
    public class RealAccountSettingsEntity : AccountSettingsEntityBase
	{
		public RealAccountSettingsEntity(CurrencyBoolDictionaryEntity canReceiveTransactionOffersFor,
										 CurrencyBoolDictionaryEntity canCreateTransactionOffersFor,
										 CurrencyBoolDictionaryEntity canBeMiddlemanFor,
										  CurrencyBoolDictionaryEntity canBeDepositAccountFor) : base(canReceiveTransactionOffersFor,
																								canCreateTransactionOffersFor,
																								canBeMiddlemanFor)
		{
			CanBeDepositAccountFor = canBeDepositAccountFor;
		}

		public RealAccountSettingsEntity() { }
		protected RealAccountSettingsEntity(RealAccountSettingsEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			CanBeDepositAccountFor = from.CanBeDepositAccountFor.CloneAsT(circularReferenceHelperDictionary);
		}

		public virtual CurrencyBoolDictionaryEntity CanBeDepositAccountFor { get; }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new RealAccountSettingsEntity(this, circularReferenceHelperDictionary);
		}


		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			await CanBeDepositAccountFor.SafeEncrypt(encryptor);
			await base.EncryptSelf(encryptor);
		}
		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			await CanBeDepositAccountFor.SafeDecrypt(decryptor);
			await base.DecryptSelf(decryptor);
		}
	}
}
