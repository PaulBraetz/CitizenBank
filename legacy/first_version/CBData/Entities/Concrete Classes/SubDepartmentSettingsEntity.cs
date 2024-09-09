namespace CBData.Entities
{
    public class SubDepartmentSettingsEntity : DepartmentSettingsEntityBase
	{
		public SubDepartmentSettingsEntity() { }
		protected SubDepartmentSettingsEntity(SubDepartmentSettingsEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary) { }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new SubDepartmentSettingsEntity(this, circularReferenceHelperDictionary);
		}
	}
}
