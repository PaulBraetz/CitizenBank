using CBData.Abstractions;

using PBData.Entities;

using System;
using System.Collections.Generic;

namespace CBData.Entities
{
	public abstract class DepartmentSettingsEntityBase<TDepartment> : SettingsEntityBase<TDepartment>, IDepartmentSettingsEntity<TDepartment>
		where TDepartment : DepartmentEntityBase
	{
		protected DepartmentSettingsEntityBase(TDepartment owner) : base(owner) { }

		protected DepartmentSettingsEntityBase() { }
		protected DepartmentSettingsEntityBase(DepartmentSettingsEntityBase<TDepartment> from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			InviteOnly = from.InviteOnly;
		}

		public virtual Boolean InviteOnly { get; set; } = true;
	}
}
