
using CBCommon.Extensions;

using CBData.Abstractions;
using CBData.Entities;
using PBApplication.Extensions;

using PBData.Abstractions;
using PBData.Access;

using System;
using System.Linq;

namespace CBApplication.Extensions
{
	internal static class EntityExtensions
	{
		public static Decimal CalculateCreditScore(this CreditScoreEntity creditScore)
		{
			return creditScore.DiscrepancyProbability * creditScore.DiscrepancyValueAverage;
		}

		public static String GetFormattedTax<TCreditor, TDebtor, TCreator, TRecipient>(this ITransactionContractEntity<TCreditor, TDebtor, TCreator, TRecipient> contract)
			where TCreditor : IAccountEntity
			where TDebtor : IAccountEntity
			where TCreator : IAccountEntity
			where TRecipient : IAccountEntity
		{
			return (contract.Gross - contract.Net).ToFormattedCurrency(contract.Currency) + " (" + contract.Tax.ToFormattedString("%") + ")";
		}
		public static Boolean BookingIsPossible(this TargetTransactionContractEntity target, Decimal value, Boolean forCreditor)
		{
			return ((forCreditor ? target.CreditorBookings : target.DebtorBookings).Sum(b => b.Value) + value) <= (forCreditor ? target.Net : target.Gross);
		}

		public static Boolean HoldsManagerRightImplicitlyRecursively<TCreditor, TDebtor, TCreator, TRecipient>(this IEntity holder, IConnection connection, ITransactionEntity<TCreditor, TDebtor, TCreator, TRecipient> value)
			where TCreditor : IAccountEntity
			where TDebtor : IAccountEntity
			where TCreator : IAccountEntity
			where TRecipient : IAccountEntity
		{
			return holder.HoldsManagerRightImplicitlyRecursively(connection, value.Creditor) ||
				holder.HoldsManagerRightImplicitlyRecursively(connection, value.Debtor) ||
				holder.HoldsManagerRightImplicitlyRecursively(connection, value.Creator) ||
				holder.HoldsManagerRightImplicitlyRecursively(connection, value.Recipient);
		}
		public static Boolean HoldsObserverRightImplicitlyRecursively<TCreditor, TDebtor, TCreator, TRecipient>(this IEntity holder, IConnection connection, ITransactionEntity<TCreditor, TDebtor, TCreator, TRecipient> value)
			where TCreditor : IAccountEntity
			where TDebtor : IAccountEntity
			where TCreator : IAccountEntity
			where TRecipient : IAccountEntity
		{
			return holder.HoldsManagerRightImplicitlyRecursively(connection, value);
		}
	}
}
