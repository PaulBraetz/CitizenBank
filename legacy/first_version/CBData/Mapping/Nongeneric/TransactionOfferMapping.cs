using CBData.Entities;

namespace CBData.Mapping
{
	internal class TransactionOfferMapping : TransactionMappingBase<TransactionOfferEntity, AccountEntityBase, AccountEntityBase, AccountEntityBase, AccountEntityBase>
	{
		public TransactionOfferMapping()
		{
			References(m => m.SourceTransactionContract).Cascade.DeleteOrphan();
			Map(m => m.RecipientAnswer);
			Map(m => m.CreatorConfirmation);
		}
	}
}
