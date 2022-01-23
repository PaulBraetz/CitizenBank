using CBData.Entities;

namespace CBData.Mapping
{
	internal class SourceTransactionContractMapping : TransactionContractMappingBase<SourceTransactionContractEntity, AccountEntityBase, AccountEntityBase, AccountEntityBase, AccountEntityBase>
	{
		public SourceTransactionContractMapping()
		{
			HasMany(m => m.TargetTransactionContracts).Cascade.AllDeleteOrphan();
		}
	}
}
