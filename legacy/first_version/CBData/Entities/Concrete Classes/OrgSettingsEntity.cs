namespace CBData.Entities
{
    public class OrgSettingsEntity : DepartmentSettingsEntityBase
	{
		public OrgSettingsEntity() { }
		protected OrgSettingsEntity(OrgSettingsEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary) { }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new OrgSettingsEntity(this, circularReferenceHelperDictionary);
		}
	}
}
