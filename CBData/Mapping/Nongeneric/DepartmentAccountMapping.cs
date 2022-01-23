using CBData.Entities;

using FluentNHibernate.Mapping;

namespace CBData.Mapping
{
	internal class DepartmentAccountMapping : SubclassMap<DepartmentAccountEntity>
	{
		public DepartmentAccountMapping()
		{
			References(m => m.Department);
		}
	}
}
