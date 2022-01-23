using CBData.Entities;

using FluentNHibernate.Mapping;

namespace CBData.Mapping
{
	internal class RealAccountMapping : SubclassMap<RealAccountEntity>
	{
		public RealAccountMapping()
		{
			References(m => m.Owner);
		}
	}
}
