
using CBData.Entities;

using PBData.Abstractions;

using System;

using static CBCommon.Enums.CitizenBankEnums;

namespace CBData.Abstractions
{
	public interface ITransactionEntity<TCreditor, TDebtor, TCreator, TRecipient> : IExpiringEntity, IHasCurrency<CurrencyEntity>, IHasCreator<TCreator>, IHasTags
		where TCreditor : IAccountEntity
		where TDebtor : IAccountEntity
		where TCreator : IAccountEntity
		where TRecipient : IAccountEntity
	{
		TCreditor Creditor { get; }
		TDebtor Debtor { get; }
		TRecipient Recipient { get; }
		Decimal Net { get; }
		Decimal Gross { get; }
		Decimal Tax { get; }
		String Usage { get; }
		TransactionPartnersRelationship Relationship { get; }
		Boolean IsExposed { get; }

		void Expose(DateTimeOffset start);
	}
}
