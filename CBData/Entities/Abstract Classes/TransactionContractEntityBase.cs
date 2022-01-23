using CBData.Abstractions;

using System;
using System.Collections.Generic;

namespace CBData.Entities
{
	public abstract class TransactionContractEntityBase<TCreditor, TDebtor, TCreator, TRecipient> : TransactionEntityBase<TCreditor, TDebtor, TCreator, TRecipient>, ITransactionContractEntity<TCreditor, TDebtor, TCreator, TRecipient>
		where TCreditor : AccountEntityBase
		where TDebtor : AccountEntityBase
		where TCreator : AccountEntityBase
		where TRecipient : AccountEntityBase
	{
		protected TransactionContractEntityBase()
		{
		}

		protected TransactionContractEntityBase(TransactionContractEntityBase<TCreditor, TDebtor, TCreator, TRecipient> from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			IsBooked = from.IsBooked;
		}

		protected TransactionContractEntityBase(TCreator creator, TRecipient recipient, TCreditor creditor, TDebtor debtor, String usage, CurrencyEntity currency, Decimal net, Decimal gross, TimeSpan lifeSpan, Boolean isCollectible, Boolean expiryPaused) : base(creator, recipient, creditor, debtor, usage, currency, net, gross, lifeSpan, isCollectible, expiryPaused)
		{
		}

		protected TransactionContractEntityBase(TCreator creator, TRecipient recipient, TCreditor creditor, TDebtor debtor, String usage, Decimal tax, CurrencyEntity currency, Decimal net, TimeSpan lifeSpan, Boolean isCollectible, Boolean expiryPaused) : base(creator, recipient, creditor, debtor, usage, tax, currency, net, lifeSpan, isCollectible, expiryPaused)
		{
		}

		public abstract Boolean IsBooked { get; set; }
	}
}
