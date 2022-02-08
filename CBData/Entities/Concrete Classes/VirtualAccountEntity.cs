
using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;
using PBData.Abstractions;
using PBData.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBData.Entities
{
	public class VirtualAccountEntity : VirtualAccountEntityBase
	{
		public VirtualAccountEntity(CitizenEntity creator, String name, CreditScoreEntity creditScore) : base(creator, name, creditScore)
		{
		}

		public VirtualAccountEntity()
		{
		}
		protected VirtualAccountEntity(VirtualAccountEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
		}

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new VirtualAccountEntity(this, circularReferenceHelperDictionary);
		}
	}
}
