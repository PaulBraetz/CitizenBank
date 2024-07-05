using CBData.Abstractions;
using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;
using PBData.Abstractions;
using PBData.Extensions;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CBCommon.Enums.CitizenBankEnums;

namespace CBData.Entities
{
	public class TransactionOfferEntity : TransactionEntityBase<AccountEntityBase, AccountEntityBase, AccountEntityBase, AccountEntityBase>, IExpiringEntity
	{
		public TransactionOfferEntity(SourceTransactionContractEntity sourceTransactionContract,
									  TimeSpan lifeSpan) : base(sourceTransactionContract.Creator,
										  sourceTransactionContract.Recipient,
								sourceTransactionContract.Creditor,
								sourceTransactionContract.Debtor,
								sourceTransactionContract.Usage,
								sourceTransactionContract.Currency,
								sourceTransactionContract.Net,
								sourceTransactionContract.Gross,
								lifeSpan,
								true,
								false)
		{
			SourceTransactionContract = sourceTransactionContract;
		}

		public TransactionOfferEntity() : base() { }
		protected TransactionOfferEntity(TransactionOfferEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			SourceTransactionContract = from.SourceTransactionContract.CloneAsT(circularReferenceHelperDictionary);
			RecipientAnswer = from.RecipientAnswer;
			CreatorConfirmation = from.CreatorConfirmation;
		}

		public virtual SourceTransactionContractEntity SourceTransactionContract { get; set; }
		public virtual TransactionOfferAnswer RecipientAnswer { get; set; }
		public virtual TransactionOfferAnswer CreatorConfirmation { get; set; }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new TransactionOfferEntity(this, circularReferenceHelperDictionary);
		}
		public virtual TransactionOfferEntity CloneFor<TAccount>(TAccount account)
			where TAccount : IAccountEntity
		{
			var retVal = this.CloneAsT();
			retVal.SourceTransactionContract = retVal.SourceTransactionContract.CloneFor(account);
			return retVal;
		}

		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			await SourceTransactionContract.SafeEncrypt(encryptor);
			await base.EncryptSelf(encryptor);
		}
		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			await SourceTransactionContract.SafeDecrypt(decryptor);
			await base.DecryptSelf(decryptor);
		}
	}
}
