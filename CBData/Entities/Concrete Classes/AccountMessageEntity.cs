using PBCommon.Globalization;
using PBData.Entities;

using System;
using System.Collections.Generic;

namespace CBData.Entities
{
	public class AccountMessageEntity : MessageEntityBase<AccountEntityBase>
	{
		public AccountMessageEntity(AccountEntityBase creator, IEnumerable<AccountEntityBase> recipients, LocalizableFormattableString
			message, TimeSpan lifeSpan, Boolean expiryPaused)
			: base(creator, recipients, message, lifeSpan, expiryPaused)
		{
		}

		public AccountMessageEntity() { }
		protected AccountMessageEntity(AccountMessageEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
		}

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new AccountMessageEntity(this, circularReferenceHelperDictionary);
		}
	}
}
