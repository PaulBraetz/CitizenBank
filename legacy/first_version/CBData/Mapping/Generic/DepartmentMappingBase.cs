using CBData.Abstractions;

namespace CBData.Mapping
{
    public class DepartmentMappingBase<TDepartment> : NamedMappingBase<TDepartment>
		where TDepartment : IDepartmentEntity
	{
		public DepartmentMappingBase()
		{
			References(m => m.Creator);
			HasMany(m => m.SubDepartments).Cascade.DeleteOrphan();
			HasManyToMany(m => m.Tags);
			HasManyToMany(m => m.PriorityTags);
		}
	}
}
