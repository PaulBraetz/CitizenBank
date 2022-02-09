using CBApplication.Services.Abstractions;

using CBCommon.Extensions;

using CBData.Abstractions;
using CBData.Entities;

using PBApplication.Services.Abstractions;

using PBData.Abstractions;
using PBData.Access;
using PBData.Entities;

using System;
using System.Linq;

namespace CBApplication.Extensions
{
	internal static class EntityExtensions
	{
		private static TTo As<TTo>(this IEntity entity)
			where TTo : IEntity
		{
			return entity is TTo entityOfTTo ? entityOfTTo : default;
		}
		public static TTo As<TTo>(this IEntity entity, IConnection connection)
			where TTo : IEntity
		{
			return entity is TTo entityOfTTo ? entityOfTTo : connection.Query<TTo>().SingleOrDefault(e => e.Id == entity.Id);
		}

		private static Boolean Is<TTo>(this IEntity entity, out TTo result)
			where TTo : IEntity
		{
			result = entity.As<TTo>();
			return result != null;
		}
		public static Boolean Is<TTo>(this IEntity entity, out TTo result, IConnection connection)
			where TTo : IEntity
		{
			result = entity.As<TTo>(connection);
			return result != null;
		}
		public static Boolean Is<TTo>(this IEntity entity, IConnection connection)
			where TTo : IEntity
		{
			return entity.As<TTo>(connection) != null;
		}

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
	}
}
