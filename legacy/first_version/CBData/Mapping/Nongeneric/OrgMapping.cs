using CBData.Entities;

namespace CBData.Mapping
{
	internal class OrgMapping : DepartmentMappingBase<OrgEntity>
	{
		public OrgMapping()
		{
			Map(m => m.SID).Length(32768);
		}
	}
}
