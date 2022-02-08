using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;
using PBData.Abstractions;
using PBData.Entities;
using PBData.Extensions;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CBData.Entities
{

	public class CitizenEntity : NamedEntityBase
	{
		public CitizenEntity(String name) : base(name) { }

		public CitizenEntity() { }
		protected CitizenEntity(CitizenEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
		}

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new CitizenEntity(this, circularReferenceHelperDictionary);
		}
	}
}
