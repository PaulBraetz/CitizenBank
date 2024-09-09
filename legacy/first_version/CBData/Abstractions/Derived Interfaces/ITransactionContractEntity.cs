namespace CBData.Abstractions
{
    public interface ITransactionContractEntity<TCreditor, TDebtor, TCreator, TRecipient> : ITransactionEntity<TCreditor, TDebtor, TCreator, TRecipient>
		where TCreditor : IAccountEntity
		where TDebtor : IAccountEntity
		where TCreator : IAccountEntity
		where TRecipient : IAccountEntity
	{
		public Boolean IsBooked { get; }
	}
}
