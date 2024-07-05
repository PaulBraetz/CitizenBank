
using System;
using System.Collections.Generic;

namespace CBData.Entities
{
	public class RealAccountEntity : AccountEntityBase
	{
		public RealAccountEntity(CitizenEntity creator, CreditScoreEntity creditScore) : base(creator, creator.Name, creditScore)
		{
		}

		public RealAccountEntity()
		{
		}
		protected RealAccountEntity(RealAccountEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
		}

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new RealAccountEntity(this, circularReferenceHelperDictionary);
		}
	}
}
