using CBData.Entities;

using FluentNHibernate.Mapping;

namespace CBData.Mapping
{
	internal class VirtualAccountMapping : AccountMappingBase<VirtualAccountEntity>
	{
		public VirtualAccountMapping()
		{
			HasMany(m => m.DepositReferences);
		}
	}
}
