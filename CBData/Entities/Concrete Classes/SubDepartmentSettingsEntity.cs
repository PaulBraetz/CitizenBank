using System;
using System.Collections.Generic;

namespace CBData.Entities
{
	public class SubDepartmentSettingsEntity : DepartmentSettingsEntityBase<SubDepartmentEntity>
	{
		public SubDepartmentSettingsEntity(SubDepartmentEntity owner) : base(owner) { }

		public SubDepartmentSettingsEntity() { }
		protected SubDepartmentSettingsEntity(SubDepartmentSettingsEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary) { }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new SubDepartmentSettingsEntity(this, circularReferenceHelperDictionary);
		}
	}
}
