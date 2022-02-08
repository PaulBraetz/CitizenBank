using System;
using System.Collections.Generic;

namespace CBData.Entities
{
	public class DepartmentAccountSettingsEntity : VirtualAccountSettingsEntityBase
	{
		public DepartmentAccountSettingsEntity(CurrencyBoolDictionaryEntity canReceiveTransactionOffersFor,
											   CurrencyBoolDictionaryEntity canCreateTransactionOffersFor,
											   CurrencyBoolDictionaryEntity canBeMiddlemanFor) : base(canReceiveTransactionOffersFor,
																									  canCreateTransactionOffersFor,
																									  canBeMiddlemanFor)
		{
		}

		public DepartmentAccountSettingsEntity() { }
		protected DepartmentAccountSettingsEntity(DepartmentAccountSettingsEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
		}

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new DepartmentAccountSettingsEntity(this, circularReferenceHelperDictionary);
		}
	}
}
