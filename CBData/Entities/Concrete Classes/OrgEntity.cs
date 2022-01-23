using System;
using System.Collections.Generic;

namespace CBData.Entities
{
	public class OrgEntity : DepartmentEntityBase
	{
		public OrgEntity(String name, String sid) : base(name)
		{
			SID = sid;
		}

		public OrgEntity() { }
		protected OrgEntity(OrgEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			SID = from.SID;
		}

		public virtual String SID { get; set; }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new OrgEntity(this, circularReferenceHelperDictionary);
		}
	}
}
