using PBCommon.Encryption.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CBData.Entities
{

	public class VirtualAccountSettingsEntity : VirtualAccountSettingsEntityBase
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
		}

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new VirtualAccountSettingsEntity(this, circularReferenceHelperDictionary);
		}
	}
}
