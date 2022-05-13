using PBData.Abstractions;
using PBData.Entities;

using System;
using System.Collections.Generic;

namespace CBData.Entities
{

	public class CitizenEntity : NamedEntityBase, ISessionAttachment
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

		public void AttachTo(UserSessionEntity session)
		{
			return;
		}

		public void DetachFrom(UserSessionEntity session)
		{
			return;
		}
	}
}
