using CBData.Entities;

namespace CBData.Mapping
{
	internal class TargetTransactionContractMapping : TransactionContractMappingBase<TargetTransactionContractEntity, RealAccountEntity, RealAccountEntity, AccountEntityBase, AccountEntityBase>
	{
		public TargetTransactionContractMapping()
		{
			HasMany(m => m.CreditorBookings).Cascade.DeleteOrphan();
			HasMany(m => m.DebtorBookings).Cascade.DeleteOrphan();
		}
	}
}
