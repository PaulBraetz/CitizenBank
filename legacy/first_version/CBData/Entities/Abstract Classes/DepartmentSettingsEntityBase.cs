﻿using CBData.Abstractions;

namespace CBData.Entities
{
    public abstract class DepartmentSettingsEntityBase : SettingsEntityBase, IDepartmentSettingsEntity
	{
		protected DepartmentSettingsEntityBase() { }
		protected DepartmentSettingsEntityBase(DepartmentSettingsEntityBase from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			InviteOnly = from.InviteOnly;
		}

		public virtual Boolean InviteOnly { get; set; } = true;
	}
}
