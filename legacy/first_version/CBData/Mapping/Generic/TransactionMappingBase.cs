using CBData.Abstractions;

namespace CBData.Mapping
{
    internal class TransactionMappingBase<TTransaction, TCreditor, TDebtor, TCreator, TRecipient> : ExpiringMappingBase<TTransaction>
		where TTransaction : ITransactionEntity<TCreditor, TDebtor, TCreator, TRecipient>
		where TCreditor : IAccountEntity
		where TDebtor : IAccountEntity
		where TCreator : IAccountEntity
		where TRecipient : IAccountEntity
	{
		public TransactionMappingBase()
		{
			References(m => m.Creator);
			References(m => m.Recipient);
			References(m => m.Creditor);
			References(m => m.Debtor);
			References(m => m.Currency);
			HasManyToMany(m => m.Tags).Cascade.DeleteOrphan();
			Map(m => m.Net);
			Map(m => m.Gross);
			Map(m => m.Tax);
			Map(m => m.Usage).Length(32768);
			Map(m => m.Relationship);
			Map(m => m.IsExposed);
		}
	}
}
