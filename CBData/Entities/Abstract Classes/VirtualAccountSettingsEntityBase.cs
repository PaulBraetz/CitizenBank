using CBData.Abstractions;

using System;
using System.Collections.Generic;

namespace CBData.Entities
{
	public abstract class VirtualAccountSettingsEntityBase : AccountSettingsEntityBase, IVirtualAccountSettingsEntity
	{
		protected VirtualAccountSettingsEntityBase() { }
		protected VirtualAccountSettingsEntityBase(VirtualAccountSettingsEntityBase from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			DepositForwardLifeSpan = from.DepositForwardLifeSpan;
			DefaultDepositAccountMapRelativeLimit = from.DefaultDepositAccountMapRelativeLimit;
			DefaultDepositAccountMapAbsoluteLimit = from.DefaultDepositAccountMapAbsoluteLimit;
		}
		protected VirtualAccountSettingsEntityBase(CurrencyBoolDictionaryEntity canReceiveTransactionOffersFor,
												 CurrencyBoolDictionaryEntity canCreateTransactionOffersFor,
												 CurrencyBoolDictionaryEntity canBeMiddlemanFor) : base(canReceiveTransactionOffersFor,
																										canCreateTransactionOffersFor,
																										canBeMiddlemanFor)
		{

		}

		public virtual TimeSpan DepositForwardLifeSpan { get; set; }
		public virtual Decimal DefaultDepositAccountMapRelativeLimit { get; set; }
		public virtual Decimal DefaultDepositAccountMapAbsoluteLimit { get; set; }
	}
}
