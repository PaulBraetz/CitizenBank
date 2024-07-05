using System;
using System.Collections.Generic;

namespace CBData.Entities
{
	public class VirtualAccountSettingsEntity : AccountSettingsEntityBase
	{
		public VirtualAccountSettingsEntity(CurrencyBoolDictionaryEntity canReceiveTransactionOffersFor,
											   CurrencyBoolDictionaryEntity canCreateTransactionOffersFor,
											   CurrencyBoolDictionaryEntity canBeMiddlemanFor) : base(canReceiveTransactionOffersFor,
																									  canCreateTransactionOffersFor,
																									  canBeMiddlemanFor)
		{
		}

		public VirtualAccountSettingsEntity() { }
		protected VirtualAccountSettingsEntity(VirtualAccountSettingsEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			DepositForwardLifeSpan = from.DepositForwardLifeSpan;
			DefaultDepositAccountMapRelativeLimit = from.DefaultDepositAccountMapRelativeLimit;
			DefaultDepositAccountMapAbsoluteLimit = from.DefaultDepositAccountMapAbsoluteLimit;
		}

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new VirtualAccountSettingsEntity(this, circularReferenceHelperDictionary);
		}
		public virtual TimeSpan DepositForwardLifeSpan { get; set; }
		public virtual Decimal DefaultDepositAccountMapRelativeLimit { get; set; }
		public virtual Decimal DefaultDepositAccountMapAbsoluteLimit { get; set; }
	}
}
