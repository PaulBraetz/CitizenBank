using CBData.Entities;

using FluentNHibernate.Mapping;

namespace CBData.Mapping
{
	internal class VirtualAccountMapping : SubclassMap<VirtualAccountEntity>
	{
		public VirtualAccountMapping()
		{
			References(m => m.Owner);
			HasMany(m => m.Admins);
		}
	}
}
