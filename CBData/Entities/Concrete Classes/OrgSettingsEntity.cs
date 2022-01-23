using System;
using System.Collections.Generic;

namespace CBData.Entities
{
	public class OrgSettingsEntity : DepartmentSettingsEntityBase<OrgEntity>
	{
		public OrgSettingsEntity(OrgEntity owner) : base(owner) { }

		public OrgSettingsEntity() { }
		protected OrgSettingsEntity(OrgSettingsEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary) { }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new OrgSettingsEntity(this, circularReferenceHelperDictionary);
		}
	}
}
