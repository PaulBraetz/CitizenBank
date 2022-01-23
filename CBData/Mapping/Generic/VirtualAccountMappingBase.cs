using CBData.Entities;

namespace CBData.Mapping
{
	internal class VirtualAccountMappingBase<TVirtualAccount> : AccountMappingBase<TVirtualAccount>
		where TVirtualAccount : VirtualAccountEntityBase
	{
		public VirtualAccountMappingBase()
		{
			HasMany(m => m.DepositReferences);
			UseUnionSubclassForInheritanceMapping();
		}
	}
}
