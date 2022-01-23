using CBApplication.Services.Abstractions;

using CBCommon.Extensions;

using CBData.Abstractions;
using CBData.Entities;

using PBApplication.Services.Abstractions;

using PBData.Abstractions;
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

		private static Boolean Administrates<TAdmin, TProperty>(this TAdmin admin, TProperty property)
			where TAdmin : IEntity
			where TProperty : IEntity
		{
			return property.Is<IHasAdmins<TAdmin>>(out IHasAdmins<TAdmin> administratable) && administratable.Admins.Any(a => a.Id == admin.Id);
		}
		public static Boolean Administrates<TAdmin, TProperty>(this TAdmin admin, TProperty property, IConnection connection)
			where TAdmin : IEntity
			where TProperty : IEntity
		{
			return admin.Administrates(property) || property.Is<IHasAdmins<TAdmin>>(out IHasAdmins<TAdmin> administratable, connection) && administratable.Admins.Any(a => a.Id == admin.Id);
		}

		private static Boolean Owns<TOwner, TProperty>(this TOwner owner, TProperty property)
			where TOwner : IEntity
			where TProperty : IEntity
		{
			return property.Is<IHasOwner<TOwner>>(out IHasOwner<TOwner> ownable) && ownable.Owner.Id == owner.Id;
		}
		public static Boolean Owns<TOwner, TProperty>(this TOwner owner, TProperty property, IConnection connection)
		   where TOwner : IEntity
		   where TProperty : IEntity
		{
			return owner.Owns(property) || property.Is<IHasOwner<TOwner>>(out IHasOwner<TOwner> ownable, connection) && ownable.Owner.Id == owner.Id;
		}

		private static Boolean Manages<TManager, TProperty>(this TManager manager, TProperty property)
			where TManager : IEntity
			where TProperty : IEntity
		{
			Boolean retVal = manager.Id == property.Id || Administrates(manager, property) || Owns(manager, property) || manager.Id == property.Id;
			Action<Boolean> setRetVal = new Action<Boolean>((b) => retVal = !retVal ? b : retVal);
			if (!retVal)
			{
				if (manager.Is<IAccountEntity>(out IAccountEntity managerAccount))
				{
					if (property.Is<ITransactionEntity<IAccountEntity, IAccountEntity, IAccountEntity, IAccountEntity>>(out ITransactionEntity<IAccountEntity, IAccountEntity, IAccountEntity, IAccountEntity> transaction))
					{
						setRetVal(transaction.Creditor.Id == managerAccount.Id || transaction.Debtor.Id == managerAccount.Id);
					}
				}
				else if (manager.Is<CitizenEntity>(out CitizenEntity citizenManager))
				{
					if (property.Is<DepartmentAccountEntity>(out DepartmentAccountEntity departmentProperty))
					{
						setRetVal(citizenManager.Manages(departmentProperty.Department));
					}
				}
				else if (property.Is<UserEntity>(out UserEntity userProperty) && manager.Is<UserEntity>(out UserEntity userManager))
				{
					return userProperty.Delegates.Any(d => d.Id == userManager.Id);
				}
			}
			return retVal;
		}
		public static Boolean Manages<TManager, TProperty>(this TManager manager, TProperty property, IConnection connection)
			where TManager : IEntity
			where TProperty : IEntity
		{
			Boolean retVal = manager.Manages(property) || Administrates(manager, property, connection) || Owns(manager, property, connection);
			Action<Boolean> setRetVal = new Action<Boolean>((b) => retVal = !retVal ? b : retVal);
			if (!retVal)
			{
				if (manager.Is<IAccountEntity>(out IAccountEntity managerAccount, connection))
				{
					if (property.Is<ITransactionEntity<IAccountEntity, IAccountEntity, IAccountEntity, IAccountEntity>>(out ITransactionEntity<IAccountEntity, IAccountEntity, IAccountEntity, IAccountEntity> transaction, connection))
					{
						setRetVal(transaction.Creditor.Id == managerAccount.Id || transaction.Debtor.Id == managerAccount.Id);
					}
					else if (property.Is<DepositAccountReferenceEntity>(out DepositAccountReferenceEntity depositProperty, connection))
					{
						IVirtualAccountEntity account = connection.Query<IVirtualAccountEntity>().Where(v => v.DepositReferences.Any(r => r.Id == depositProperty.Id)).Single();
						setRetVal(managerAccount.Manages(account, connection) || managerAccount.Manages(depositProperty.ReferencedAccount, connection));
					}
				}
				else if (manager.Is<CitizenEntity>(out CitizenEntity citizenManager, connection))
				{
					if (property.Is<DepartmentAccountEntity>(out DepartmentAccountEntity departmentProperty, connection))
					{
						setRetVal(citizenManager.Manages(departmentProperty.Department, connection));
					}
				}
				else if (property.Is<UserEntity>(out UserEntity userProperty, connection) && manager.Is<UserEntity>(out UserEntity userManager, connection))
				{
					return userProperty.Delegates.Any(d => d.Id == userManager.Id);
				}
			}
			return retVal;
		}

		private static Boolean CanSee<TObserver, TProperty>(this TObserver observer, TProperty property)
			where TObserver : IEntity
			where TProperty : IEntity
		{
			Boolean retVal = observer.Manages(property);
			Action<Boolean> setRetVal = new Action<Boolean>((b) => retVal = !retVal ? b : retVal);
			if (!retVal)
			{
				if (observer.Is<IAccountEntity>(out IAccountEntity accountObserver))
				{
					if (property.Is<IHasMembers<IEntity>>(out IHasMembers<IEntity> memberProperty))
					{
						setRetVal(memberProperty.Members.Any(m => observer.Manages(m)));
					}
				}
			}
			return retVal;
		}
		public static Boolean CanSee<TObserver, TProperty>(this TObserver observer, TProperty property, IConnection connection)
			where TObserver : IEntity
			where TProperty : IEntity
		{
			Boolean retVal = observer.CanSee(property) || observer.Manages(property, connection);
			Action<Boolean> setRetVal = new Action<Boolean>((b) =>
			{
				retVal = !retVal ? b : retVal;
			});
			if (!retVal)
			{
				if (observer.Is(out IAccountEntity accountObserver, connection))
				{
					if (property.Is(out IHasMembers<IEntity> memberProperty, connection))
					{
						setRetVal(memberProperty.Members.Any(m => observer.Manages(m, connection)));
					}
					if (property.Is(out IAccountEntity accountProperty, connection))
					{
						IQueryable<IHasMembers<IEntity>> commonMemberIn = connection
							.Query<IHasMembers<IEntity>>()
							.Where(hm =>
								hm.Members
								.Any(m => m.Id == accountProperty.Id) &&
								hm.Members
								.Any(m => m.Id == accountObserver.Id));
						setRetVal(commonMemberIn.Any());
					}
				}
			}
			return retVal;
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
