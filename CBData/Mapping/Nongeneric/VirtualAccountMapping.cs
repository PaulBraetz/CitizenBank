using CBData.Entities;

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
