using CBData.Abstractions;

namespace CBData.Mapping
{
	internal class TransactionContractMappingBase<TTransaction, TCreditor, TDebtor, TCreator, TRecipient> : TransactionMappingBase<TTransaction, TCreditor, TDebtor, TCreator, TRecipient>
		where TTransaction : ITransactionContractEntity<TCreditor, TDebtor, TCreator, TRecipient>
		where TCreditor : IAccountEntity
		where TDebtor : IAccountEntity
		where TCreator : IAccountEntity
		where TRecipient : IAccountEntity
	{
		public TransactionContractMappingBase()
		{
			Map(m => m.IsBooked);
		}
	}
}
