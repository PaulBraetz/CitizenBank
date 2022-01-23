using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;
using PBData.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CBCommon.Enums.CitizenBankEnums;

namespace CBData.Entities
{
	public class TargetTransactionContractEntity : TransactionContractEntityBase<RealAccountEntity, RealAccountEntity, AccountEntityBase, AccountEntityBase>
	{
		public TargetTransactionContractEntity(AccountEntityBase creator,
												 AccountEntityBase recipient,
												 RealAccountEntity creditor,
												 RealAccountEntity debtor,
												 String usage,
												 Decimal tax,
												 CurrencyEntity currency,
												 Decimal net,
												 TransactionPartnersRelationship relationship,
												 TimeSpan lifeSpan) : base(creator,
																			 recipient,
																			 creditor,
																			 debtor,
																			 usage,
																			 tax,
																			 currency,
																			 net,
																			 lifeSpan,
																			 false,
																			 false)
		{
			Relationship = relationship;
			if (Creditor.Id == Debtor.Id)
			{
				Tax = 0;
				Gross = Net;
			}
			CreditorBookings = new List<BookingEntity>();
			DebtorBookings = new List<BookingEntity>();
		}

		public TargetTransactionContractEntity() { }
		protected TargetTransactionContractEntity(TargetTransactionContractEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			CreditorBookings = from.CreditorBookings.CloneAsT(circularReferenceHelperDictionary).ToList();
			DebtorBookings = from.DebtorBookings.CloneAsT(circularReferenceHelperDictionary).ToList();
			LifeSpan = from.LifeSpan;
		}

		public override Boolean IsBooked { get => Creditor.Id == Debtor.Id || CreditorBookings.Sum(b => b.Value) == Net && DebtorBookings.Sum(b => b.Value) == Net; set => _ = value; }
		public virtual ICollection<BookingEntity> CreditorBookings { get; set; }
		public virtual ICollection<BookingEntity> DebtorBookings { get; set; }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new TargetTransactionContractEntity(this, circularReferenceHelperDictionary);
		}

		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			await Task.WhenAll(
				CreditorBookings.SafeEncrypt(encryptor),
				DebtorBookings.SafeEncrypt(encryptor));
			await base.EncryptSelf(encryptor);
		}
		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			await Task.WhenAll(
				CreditorBookings.SafeDecrypt(decryptor),
				DebtorBookings.SafeDecrypt(decryptor));
			await base.DecryptSelf(decryptor);
		}
	}
}
