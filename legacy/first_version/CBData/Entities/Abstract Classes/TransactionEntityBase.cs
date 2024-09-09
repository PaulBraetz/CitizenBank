using CBCommon.Extensions;

using CBData.Abstractions;

using static CBCommon.Enums.CitizenBankEnums;

namespace CBData.Entities
{
    public abstract class TransactionEntityBase<TCreditor, TDebtor, TCreator, TRecipient> : ExpiringEntityBase, ITransactionEntity<TCreditor, TDebtor, TCreator, TRecipient>
		where TCreditor : AccountEntityBase
		where TDebtor : AccountEntityBase
		where TCreator : AccountEntityBase
		where TRecipient : AccountEntityBase
	{
		protected TransactionEntityBase() { }
		protected TransactionEntityBase(TransactionEntityBase<TCreditor, TDebtor, TCreator, TRecipient> from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			Creator = from.Creator.CloneAsT(circularReferenceHelperDictionary);
			Recipient = from.Recipient.CloneAsT(circularReferenceHelperDictionary);
			Creditor = from.Creditor.CloneAsT(circularReferenceHelperDictionary);
			Debtor = from.Debtor.CloneAsT(circularReferenceHelperDictionary);
			Currency = from.Currency.CloneAsT(circularReferenceHelperDictionary);
			Tags = from.Tags.CloneAsT(circularReferenceHelperDictionary).ToList();
			Net = from.Net;
			Gross = from.Gross;
			Tax = from.Tax;
			Usage = from.Usage;
			Relationship = from.Relationship;
			IsExposed = from.IsExposed;
		}
		protected TransactionEntityBase(TCreator creator,
										TRecipient recipient,
										TCreditor creditor,
										TDebtor debtor,
										String usage,
										CurrencyEntity currency,
										Decimal net,
										Decimal gross,
										TimeSpan lifeSpan,
										Boolean isCollectible,
										Boolean expiryPaused) : base(lifeSpan, isCollectible, expiryPaused)
		{
			Creator = creator;
			Recipient = recipient;
			Creditor = creditor;
			Debtor = debtor;
			Net = net;
			Gross = gross;
			Currency = currency;
			Usage = usage;
			Tax = (Gross - Net) / Net;
			Tags = new List<TagEntity>();
			IsCollectible = false;
		}
		protected TransactionEntityBase(TCreator creator,
										TRecipient recipient,
										TCreditor creditor,
										TDebtor debtor,
										String usage,
										Decimal tax,
										CurrencyEntity currency,
										Decimal net,
										TimeSpan lifeSpan,
										Boolean isCollectible,
										Boolean expiryPaused) : base(lifeSpan, isCollectible, expiryPaused)
		{
			Creator = creator;
			Recipient = recipient;
			Creditor = creditor;
			Debtor = debtor;
			Net = net;
			Tax = tax;
			Gross = (Net + Net * Tax).RoundCIG();
			Tax = (Gross - Net) / Net;
			Currency = currency;
			Usage = usage;
			Tags = new List<TagEntity>();
			IsCollectible = false;
		}

		public virtual TCreator Creator { get; set; }
		public virtual TRecipient Recipient { get; set; }
		public virtual TCreditor Creditor { get; set; }
		public virtual TDebtor Debtor { get; set; }
		public virtual CurrencyEntity Currency { get; set; }
		public virtual ICollection<TagEntity> Tags { get; set; }
		public virtual Decimal Net { get; set; }
		public virtual Decimal Gross { get; set; }
		public virtual Decimal Tax { get; set; }
		public virtual String Usage { get; set; }
		public virtual TransactionPartnersRelationship Relationship { get; set; }
		public virtual Boolean IsExposed { get => ExpirationDate > TimeManager.Now; protected set => _ = value; }
		public override Boolean ExpiryPaused { get; set; }

		public virtual void Expose(DateTimeOffset start)
		{
			RefreshOn(start);
			IsExposed = true;
		}
		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			await Task.WhenAll(
				Creator.SafeEncrypt(encryptor),
				Recipient.SafeEncrypt(encryptor),
				Creditor.SafeEncrypt(encryptor),
				Debtor.SafeEncrypt(encryptor),
				Currency.SafeEncrypt(encryptor),
				Tags.SafeEncrypt(encryptor));
			await base.EncryptSelf(encryptor);
		}
		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			await Task.WhenAll(
				Creator.SafeDecrypt(decryptor),
				Recipient.SafeDecrypt(decryptor),
				Creditor.SafeDecrypt(decryptor),
				Debtor.SafeDecrypt(decryptor),
				Currency.SafeDecrypt(decryptor),
				Tags.SafeDecrypt(decryptor));
			await base.DecryptSelf(decryptor);
		}
	}
}
