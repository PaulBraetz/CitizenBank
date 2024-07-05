using PBData.Entities;

using System;
using System.Collections.Generic;

namespace CBData.Entities
{
	public class CitizenSettingsEntity : SettingsEntityBase
	{
		public CitizenSettingsEntity()
		{
			CanBeRecruitedAsDepartmentAdmin = true;
			CanBeRecruitedAsAccountAdmin = true;
		}
		protected CitizenSettingsEntity(CitizenSettingsEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			CanBeRecruitedAsDepartmentAdmin = from.CanBeRecruitedAsDepartmentAdmin;
			CanBeRecruitedAsAccountAdmin = from.CanBeRecruitedAsAccountAdmin;
		}

		public virtual Boolean CanBeRecruitedAsDepartmentAdmin { get; set; }
		public virtual Boolean CanBeRecruitedAsAccountAdmin { get; set; }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new CitizenSettingsEntity(this, circularReferenceHelperDictionary);
		}
	}
}
