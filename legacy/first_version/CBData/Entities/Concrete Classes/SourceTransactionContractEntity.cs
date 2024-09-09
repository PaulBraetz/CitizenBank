using CBData.Abstractions;

using static CBCommon.Enums.CitizenBankEnums;

namespace CBData.Entities
{
    public class SourceTransactionContractEntity : TransactionContractEntityBase<AccountEntityBase, AccountEntityBase, AccountEntityBase, AccountEntityBase>
	{
		public SourceTransactionContractEntity(AccountEntityBase creator,
										 AccountEntityBase recipient,
										 AccountEntityBase creditor,
										 AccountEntityBase debtor,
										 String usage,
										 CurrencyEntity currency,
										 Decimal net,
										 Decimal gross,
										 TransactionPartnersRelationship relationship) : base(creator,
																									recipient,
																									creditor,
																									debtor,
																									usage,
																									currency,
																									net,
																									gross,
																									TimeSpan.Zero,
																									false,
																									false)
		{
			Relationship = relationship;
			if (Creditor.Id == Debtor.Id)
			{
				Tax = 0;
				Gross = Net;
			}
			TargetTransactionContracts = new List<TargetTransactionContractEntity>();
		}

		public SourceTransactionContractEntity() { }
		protected SourceTransactionContractEntity(SourceTransactionContractEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			TargetTransactionContracts = from.TargetTransactionContracts.CloneAsT(circularReferenceHelperDictionary).ToList();
		}

		public override Boolean IsBooked
		{
			get => Creditor.Id == Debtor.Id || TargetTransactionContracts.All(c => c.IsBooked);
			set { }
		}
		public virtual ICollection<TargetTransactionContractEntity> TargetTransactionContracts { get; set; }
		public override TimeSpan LifeSpan
		{
			get
			{
				switch (Relationship)
				{
					case TransactionPartnersRelationship.RealToReal:
						{
							return TargetTransactionContracts.Single().LifeSpan;
						}
					case TransactionPartnersRelationship.RealToVirtual:
						{
							TimeSpan retVal = TargetTransactionContracts.Single(t => t.Relationship == TransactionPartnersRelationship.RealToForward).LifeSpan;
							if (TargetTransactionContracts.Any(t => t.Relationship == TransactionPartnersRelationship.ForwardToDeposit))
							{
								retVal += TargetTransactionContracts.First(t => t.Relationship == TransactionPartnersRelationship.ForwardToDeposit).LifeSpan;
							}
							return retVal;
						}
					case TransactionPartnersRelationship.VirtualToReal:
						{
							TimeSpan retVal = TargetTransactionContracts.Single(t => t.Relationship == TransactionPartnersRelationship.ForwardToReal).LifeSpan;
							if (TargetTransactionContracts.Any(t => t.Relationship == TransactionPartnersRelationship.ForwardToDeposit))
							{
								retVal += TargetTransactionContracts.First(t => t.Relationship == TransactionPartnersRelationship.DepositToForward).LifeSpan;
							}
							return retVal;
						}
					case TransactionPartnersRelationship.VirtualToVirtual:
						{
							TimeSpan retVal = TargetTransactionContracts.Single(t => t.Relationship == TransactionPartnersRelationship.ForwardToForward).LifeSpan;
							if (TargetTransactionContracts.Any(t => t.Relationship == TransactionPartnersRelationship.ForwardToDeposit))
							{
								retVal += TargetTransactionContracts.First(t => t.Relationship == TransactionPartnersRelationship.ForwardToDeposit).LifeSpan;
							}
							if (TargetTransactionContracts.Any(t => t.Relationship == TransactionPartnersRelationship.DepositToForward))
							{
								retVal += TargetTransactionContracts.First(t => t.Relationship == TransactionPartnersRelationship.DepositToForward).LifeSpan;
							}
							return retVal;
						}
					case TransactionPartnersRelationship.Equalizing:
						{
							return TargetTransactionContracts.First(t => t.Relationship == TransactionPartnersRelationship.EqualizingDepositToForward).LifeSpan +
								TargetTransactionContracts.First(t => t.Relationship == TransactionPartnersRelationship.EqualizingDepositToForward).LifeSpan;
						}
					default:
						return default;
				}
			}
			set { }
		}

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new SourceTransactionContractEntity(this, circularReferenceHelperDictionary);
		}
		public virtual SourceTransactionContractEntity CloneFor<TAccount>(TAccount account)
			where TAccount : IAccountEntity
		{
			Boolean isCreditor = account.Id == Creditor.Id;
			Boolean isDebtor = account.Id == Debtor.Id;

			if (!isCreditor && !isDebtor)
			{
				throw new ArgumentException("Account is neither creditor nor debtor.");
			}

			var retVal = this.CloneAsT();

			Boolean check(TargetTransactionContractEntity t)
			{
				return (Relationship == TransactionPartnersRelationship.RealToReal &&
							(isCreditor || isDebtor)) ||
						(Relationship == TransactionPartnersRelationship.RealToVirtual &&
							(isCreditor ||
							(isDebtor && t.Relationship == TransactionPartnersRelationship.RealToForward))) ||
						(Relationship == TransactionPartnersRelationship.VirtualToReal &&
							(isDebtor ||
							(isCreditor && t.Relationship == TransactionPartnersRelationship.ForwardToReal))) ||
						(Relationship == TransactionPartnersRelationship.VirtualToVirtual &&
							((isCreditor &&
								(t.Relationship == TransactionPartnersRelationship.ForwardToForward || t.Relationship == TransactionPartnersRelationship.ForwardToDeposit)) ||
							(isDebtor &&
								(t.Relationship == TransactionPartnersRelationship.ForwardToForward || t.Relationship == TransactionPartnersRelationship.DepositToForward)))) ||
						(Relationship == TransactionPartnersRelationship.Equalizing &&
							isCreditor);
			}

			retVal.TargetTransactionContracts = TargetTransactionContracts.Where(check).ToList();

			return retVal;
		}


		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			await TargetTransactionContracts.SafeEncrypt(encryptor);
			await base.EncryptSelf(encryptor);
		}
		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			await TargetTransactionContracts.SafeDecrypt(decryptor);
			await base.DecryptSelf(decryptor);
		}
	}
}
