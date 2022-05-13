using System;
using System.Collections.Generic;

namespace CBData.Entities
{
	public class SubDepartmentEntity : DepartmentEntityBase
	{
		public SubDepartmentEntity(CitizenEntity creator, String name) : base(creator, name)
		{
		}

		public SubDepartmentEntity() { }
		protected SubDepartmentEntity(SubDepartmentEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
		}

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new SubDepartmentEntity(this, circularReferenceHelperDictionary);
		}
	}
}
