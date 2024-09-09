using CBData.Abstractions;
using CBData.Entities;

using System.Linq.Expressions;

using static CBCommon.Enums.CitizenBankEnums;

namespace CBApplication.Extensions
{
    internal static class Linq
	{
		public static IOrderedQueryable<T> OrderTransactions<T, TCreditor, TDebtor, TCreator, TRecipient>(this IQueryable<T> query, SimpleTransactionContractProperty orderBy, Boolean orderDescending)
			where T : ITransactionEntity<TCreditor, TDebtor, TCreator, TRecipient>
			where TCreditor : AccountEntityBase
			where TDebtor : AccountEntityBase
			where TCreator : AccountEntityBase
			where TRecipient : AccountEntityBase
		{
			switch (orderBy)
			{
				case SimpleTransactionContractProperty.Created:
					{
						Expression<Func<T, DateTimeOffset>> expr = t => t.CreationDate;
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case SimpleTransactionContractProperty.Gross:
					{
						Expression<Func<T, Decimal>> expr = t => t.Gross;
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case SimpleTransactionContractProperty.Net:
					{
						Expression<Func<T, Decimal>> expr = t => t.Net;
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case SimpleTransactionContractProperty.TaxPercentage:
					{
						Expression<Func<T, Decimal>> expr = t => t.Tax;
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case SimpleTransactionContractProperty.TaxValue:
					{
						Expression<Func<T, Decimal>> expr = t => t.Gross - t.Net;
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case SimpleTransactionContractProperty.ExpirationDate:
					{
						Expression<Func<T, DateTimeOffset>> expr = t => t.ExpirationDate;
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case SimpleTransactionContractProperty.CreditorName:
					{
						Expression<Func<T, String>> expr = t => t.Creditor.Name;
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case SimpleTransactionContractProperty.DebtorName:
					{
						Expression<Func<T, String>> expr = t => t.Debtor.Name;
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case SimpleTransactionContractProperty.Usage:
					{
						Expression<Func<T, String>> expr = t => t.Usage;
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case SimpleTransactionContractProperty.CurrencyName:
					{
						Expression<Func<T, String>> expr = t => t.Currency.Name;
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case SimpleTransactionContractProperty.CurrencyTax:
					{
						Expression<Func<T, Decimal>> expr = t => t.Currency.IngameTax;
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case SimpleTransactionContractProperty.CurrencyStatus:
					{
						Expression<Func<T, Boolean>> expr = t => t.Currency.IsActive;
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case SimpleTransactionContractProperty.TagsCount:
					{
						Expression<Func<T, Int32>> expr = s => s.Tags.Count;
						return orderDescending ? query.OrderByDescending(expr) : orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case SimpleTransactionContractProperty.Tags:
					{
						Expression<Func<T, String>> expr = s => String.Join(',', s.Tags.Select(t => t.Name));
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				default:
					{
						return (IOrderedQueryable<T>)query;

					}
			}
		}
		public static IOrderedQueryable<T> OrderSourceTransactions<T>(this IQueryable<T> query, IAccountEntity currentAccount, AdvancedTransactionContractProperty orderBy, Boolean orderDescending)
			where T : SourceTransactionContractEntity
		{
			if (Enum.IsDefined(typeof(SimpleTransactionContractProperty), (Int32)orderBy))
			{
				return query.OrderTransactions<T, AccountEntityBase, AccountEntityBase, AccountEntityBase, AccountEntityBase>((SimpleTransactionContractProperty)(Int32)orderBy, orderDescending);
			}
			switch (orderBy)
			{
				case AdvancedTransactionContractProperty.CreditorBookingDecimal:
					{
						Expression<Func<T, Decimal>> expr = s => s.IsBooked ?
							 s.Net :
							 s.TargetTransactionContracts
								 .Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									 t.Relationship == TransactionPartnersRelationship.RealToReal :
									 s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									 t.Relationship == TransactionPartnersRelationship.RealToForward :
									 s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									 t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									 t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								 .Sum(t => t.CreditorBookings.Sum(b => b.Value));
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case AdvancedTransactionContractProperty.DebtorBookingDecimal:
					{
						Expression<Func<T, Decimal>> expr = s => s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.DebtorBookings.Sum(b => b.Value));
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case AdvancedTransactionContractProperty.AccountBookingDecimal:
					{
						Expression<Func<T, Decimal>> expr = s => s.IsBooked ?
							 s.Net :
							 s.TargetTransactionContracts
								 .Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									 t.Relationship == TransactionPartnersRelationship.RealToReal :
									 s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									 t.Relationship == TransactionPartnersRelationship.RealToForward :
									 s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									 t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									 t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								 .Sum(t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value));
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case AdvancedTransactionContractProperty.CreditorBookingPercent:
					{
						Expression<Func<T, Decimal>> expr = s => s.IsBooked ?
							 100 :
							 s.TargetTransactionContracts
								 .Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									 t.Relationship == TransactionPartnersRelationship.RealToReal :
									 s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									 t.Relationship == TransactionPartnersRelationship.RealToForward :
									 s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									 t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									 t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								 .Sum(t => t.CreditorBookings.Sum(b => b.Value)) / s.Gross;
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case AdvancedTransactionContractProperty.DebtorBookingPercent:
					{
						Expression<Func<T, Decimal>> expr = s => s.IsBooked ?
							100 :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.DebtorBookings.Sum(b => b.Value)) / s.Gross;
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case AdvancedTransactionContractProperty.AccountBookingPercent:
					{
						Expression<Func<T, Decimal>> expr = s => s.IsBooked ?
							 100 :
							 s.TargetTransactionContracts
								 .Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									 t.Relationship == TransactionPartnersRelationship.RealToReal :
									 s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									 t.Relationship == TransactionPartnersRelationship.RealToForward :
									 s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									 t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									 t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								 .Sum(t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value)) / s.Gross;
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				default:
					{
						return (IOrderedQueryable<T>)query;
					}
			}
		}
		public static IOrderedQueryable<T> OrderTargetTransactions<T>(this IQueryable<T> query, IAccountEntity currentAccount, AdvancedTransactionContractProperty orderBy, Boolean orderDescending)
			where T : TargetTransactionContractEntity
		{
			if (Enum.IsDefined(typeof(SimpleTransactionContractProperty), (Int32)orderBy))
			{
				return query.OrderTransactions<T, RealAccountEntity, RealAccountEntity, AccountEntityBase, AccountEntityBase>((SimpleTransactionContractProperty)(Int32)orderBy, orderDescending);
			}
			switch (orderBy)
			{
				case AdvancedTransactionContractProperty.CreditorBookingDecimal:
					{
						Expression<Func<T, Decimal>> expr = t => t.CreditorBookings.Sum(b => b.Value);
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case AdvancedTransactionContractProperty.DebtorBookingDecimal:
					{
						Expression<Func<T, Decimal>> expr = t => t.DebtorBookings.Sum(b => b.Value);
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case AdvancedTransactionContractProperty.AccountBookingDecimal:
					{
						Expression<Func<T, Decimal>> expr = t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value);
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case AdvancedTransactionContractProperty.CreditorBookingPercent:
					{
						Expression<Func<T, Decimal>> expr = t => t.CreditorBookings.Sum(b => b.Value) / t.Gross;
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case AdvancedTransactionContractProperty.DebtorBookingPercent:
					{
						Expression<Func<T, Decimal>> expr = t => t.DebtorBookings.Sum(b => b.Value) / t.Gross;
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				case AdvancedTransactionContractProperty.AccountBookingPercent:
					{
						Expression<Func<T, Decimal>> expr = t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value) / t.Gross;
						return orderDescending ? query.OrderByDescending(expr) : query.OrderBy(expr);

					}
				default:
					{
						return (IOrderedQueryable<T>)query;

					}
			}
		}
		public static IQueryable<T> FilterTransactions<T, TCreditor, TDebtor, TCreator, TRecipient>(this IQueryable<T> query, SimpleTransactionContractProperty filterProperty, TransactionContractFilterComparator filterComparator, String filterValue)
			where T : ITransactionEntity<TCreditor, TDebtor, TCreator, TRecipient>
			where TCreditor : AccountEntityBase
			where TDebtor : AccountEntityBase
			where TCreator : AccountEntityBase
			where TRecipient : AccountEntityBase
		{
			filterValue ??= String.Empty;
			Boolean validDate = DateTimeOffset.TryParse(filterValue, out DateTimeOffset dateFilterValue);
			Boolean validDecimal = Decimal.TryParse(filterValue, out Decimal decimalFilterValue);
			String[] tags = filterValue.Split(',');
			Boolean validBool = Boolean.TryParse(filterValue, out Boolean boolFilterValue);
			switch (filterProperty)
			{
				case SimpleTransactionContractProperty.CreditorName:
					return filterComparator switch
					{
						TransactionContractFilterComparator.greaterThan => query.Where(t => t.Creditor.Name.ToLower().CompareTo(filterValue.ToLower()) > 0),
						TransactionContractFilterComparator.lessThan => query.Where(t => t.Creditor.Name.ToLower().CompareTo(filterValue.ToLower()) < 0),
						TransactionContractFilterComparator.greaterThanOrEqual => query.Where(t => t.Creditor.Name.ToLower().CompareTo(filterValue.ToLower()) >= 0),
						TransactionContractFilterComparator.lessThanOrEqual => query.Where(t => t.Creditor.Name.ToLower().CompareTo(filterValue.ToLower()) <= 0),
						TransactionContractFilterComparator.equal => query.Where(t => t.Creditor.Name.ToLower().Equals(filterValue.ToLower())),
						TransactionContractFilterComparator.notEqual => query.Where(t => !t.Creditor.Name.ToLower().Equals(filterValue.ToLower())),
						TransactionContractFilterComparator.contains => query.Where(t => t.Creditor.Name.ToLower().Contains(filterValue.ToLower())),
						TransactionContractFilterComparator.doesNotContain => query.Where(t => !t.Creditor.Name.ToLower().Contains(filterValue.ToLower())),
						_ => query
					};
				case SimpleTransactionContractProperty.DebtorName:
					return filterComparator switch
					{
						TransactionContractFilterComparator.greaterThan => query.Where(t => t.Debtor.Name.ToLower().CompareTo(filterValue.ToLower()) > 0),
						TransactionContractFilterComparator.lessThan => query.Where(t => t.Debtor.Name.ToLower().CompareTo(filterValue.ToLower()) < 0),
						TransactionContractFilterComparator.greaterThanOrEqual => query.Where(t => t.Debtor.Name.ToLower().CompareTo(filterValue.ToLower()) >= 0),
						TransactionContractFilterComparator.lessThanOrEqual => query.Where(t => t.Debtor.Name.ToLower().CompareTo(filterValue.ToLower()) <= 0),
						TransactionContractFilterComparator.equal => query.Where(t => t.Debtor.Name.ToLower().Equals(filterValue.ToLower())),
						TransactionContractFilterComparator.notEqual => query.Where(t => !t.Debtor.Name.ToLower().Equals(filterValue.ToLower())),
						TransactionContractFilterComparator.contains => query.Where(t => t.Debtor.Name.ToLower().Contains(filterValue.ToLower())),
						TransactionContractFilterComparator.doesNotContain => query.Where(t => !t.Debtor.Name.ToLower().Contains(filterValue.ToLower())),
						_ => query
					};
				case SimpleTransactionContractProperty.CreatorName:
					return filterComparator switch
					{
						TransactionContractFilterComparator.greaterThan => query.Where(t => t.Creator.Name.ToLower().CompareTo(filterValue.ToLower()) > 0),
						TransactionContractFilterComparator.lessThan => query.Where(t => t.Creator.Name.ToLower().CompareTo(filterValue.ToLower()) < 0),
						TransactionContractFilterComparator.greaterThanOrEqual => query.Where(t => t.Creator.Name.ToLower().CompareTo(filterValue.ToLower()) >= 0),
						TransactionContractFilterComparator.lessThanOrEqual => query.Where(t => t.Creator.Name.ToLower().CompareTo(filterValue.ToLower()) <= 0),
						TransactionContractFilterComparator.equal => query.Where(t => t.Creator.Name.ToLower().Equals(filterValue.ToLower())),
						TransactionContractFilterComparator.notEqual => query.Where(t => !t.Creator.Name.ToLower().Equals(filterValue.ToLower())),
						TransactionContractFilterComparator.contains => query.Where(t => t.Creator.Name.ToLower().Contains(filterValue.ToLower())),
						TransactionContractFilterComparator.doesNotContain => query.Where(t => !t.Creator.Name.ToLower().Contains(filterValue.ToLower())),
						_ => query,
					};
				case SimpleTransactionContractProperty.RecipientName:
					return filterComparator switch
					{
						TransactionContractFilterComparator.greaterThan => query.Where(t => t.Recipient.Name.ToLower().CompareTo(filterValue.ToLower()) > 0),
						TransactionContractFilterComparator.lessThan => query.Where(t => t.Recipient.Name.ToLower().CompareTo(filterValue.ToLower()) < 0),
						TransactionContractFilterComparator.greaterThanOrEqual => query.Where(t => t.Recipient.Name.ToLower().CompareTo(filterValue.ToLower()) >= 0),
						TransactionContractFilterComparator.lessThanOrEqual => query.Where(t => t.Recipient.Name.ToLower().CompareTo(filterValue.ToLower()) <= 0),
						TransactionContractFilterComparator.equal => query.Where(t => t.Recipient.Name.ToLower().Equals(filterValue.ToLower())),
						TransactionContractFilterComparator.notEqual => query.Where(t => !t.Recipient.Name.ToLower().Equals(filterValue.ToLower())),
						TransactionContractFilterComparator.contains => query.Where(t => t.Recipient.Name.ToLower().Contains(filterValue.ToLower())),
						TransactionContractFilterComparator.doesNotContain => query.Where(t => !t.Recipient.Name.ToLower().Contains(filterValue.ToLower())),
						_ => query,
					};
				case SimpleTransactionContractProperty.Usage:
					return filterComparator switch
					{
						TransactionContractFilterComparator.greaterThan => query.Where(t => t.Usage.ToLower().CompareTo(filterValue.ToLower()) > 0),
						TransactionContractFilterComparator.lessThan => query.Where(t => t.Usage.ToLower().CompareTo(filterValue.ToLower()) < 0),
						TransactionContractFilterComparator.greaterThanOrEqual => query.Where(t => t.Usage.ToLower().CompareTo(filterValue.ToLower()) >= 0),
						TransactionContractFilterComparator.lessThanOrEqual => query.Where(t => t.Usage.ToLower().CompareTo(filterValue.ToLower()) <= 0),
						TransactionContractFilterComparator.equal => query.Where(t => t.Usage.ToLower().Equals(filterValue.ToLower())),
						TransactionContractFilterComparator.notEqual => query.Where(t => !t.Usage.ToLower().Equals(filterValue.ToLower())),
						TransactionContractFilterComparator.contains => query.Where(t => t.Usage.ToLower().Contains(filterValue.ToLower())),
						TransactionContractFilterComparator.doesNotContain => query.Where(t => !t.Usage.ToLower().Contains(filterValue.ToLower())),
						_ => query,
					};
				case SimpleTransactionContractProperty.Created:
					return validDate ? filterComparator switch
					{
						TransactionContractFilterComparator.greaterThan => query.Where(t => t.CreationDate > dateFilterValue),
						TransactionContractFilterComparator.lessThan => query.Where(t => t.CreationDate < dateFilterValue),
						TransactionContractFilterComparator.greaterThanOrEqual => query.Where(t => t.CreationDate >= dateFilterValue),
						TransactionContractFilterComparator.lessThanOrEqual => query.Where(t => t.CreationDate <= dateFilterValue),
						TransactionContractFilterComparator.equal => query.Where(t => t.CreationDate == dateFilterValue),
						TransactionContractFilterComparator.notEqual => query.Where(t => t.CreationDate != dateFilterValue),
						_ => query,
					} : query;
				case SimpleTransactionContractProperty.ExpirationDate:
					return validDate ? filterComparator switch
					{
						TransactionContractFilterComparator.greaterThan => query.Where(t => t.ExpirationDate > dateFilterValue),
						TransactionContractFilterComparator.lessThan => query.Where(t => t.ExpirationDate < dateFilterValue),
						TransactionContractFilterComparator.greaterThanOrEqual => query.Where(t => t.ExpirationDate >= dateFilterValue),
						TransactionContractFilterComparator.lessThanOrEqual => query.Where(t => t.ExpirationDate <= dateFilterValue),
						TransactionContractFilterComparator.equal => query.Where(t => t.ExpirationDate == dateFilterValue),
						TransactionContractFilterComparator.notEqual => query.Where(t => t.ExpirationDate != dateFilterValue),
						_ => query,
					} : query;
				case SimpleTransactionContractProperty.Gross:
					return validDecimal ? filterComparator switch
					{
						TransactionContractFilterComparator.greaterThan => query.Where(t => t.Gross > decimalFilterValue),
						TransactionContractFilterComparator.lessThan => query.Where(t => t.Gross < decimalFilterValue),
						TransactionContractFilterComparator.greaterThanOrEqual => query.Where(t => t.Gross >= decimalFilterValue),
						TransactionContractFilterComparator.lessThanOrEqual => query.Where(t => t.Gross <= decimalFilterValue),
						TransactionContractFilterComparator.equal => query.Where(t => t.Gross == decimalFilterValue),
						TransactionContractFilterComparator.notEqual => query.Where(t => t.Gross != decimalFilterValue),
						TransactionContractFilterComparator.contains => query.Where(t => t.Gross > decimalFilterValue),
						TransactionContractFilterComparator.doesNotContain => query.Where(t => t.Gross < decimalFilterValue),
						_ => query,
					} : query;
				case SimpleTransactionContractProperty.Net:
					return validDecimal ? filterComparator switch
					{
						TransactionContractFilterComparator.greaterThan => query.Where(t => t.Net > decimalFilterValue),
						TransactionContractFilterComparator.lessThan => query.Where(t => t.Net < decimalFilterValue),
						TransactionContractFilterComparator.greaterThanOrEqual => query.Where(t => t.Net >= decimalFilterValue),
						TransactionContractFilterComparator.lessThanOrEqual => query.Where(t => t.Net <= decimalFilterValue),
						TransactionContractFilterComparator.equal => query.Where(t => t.Net == decimalFilterValue),
						TransactionContractFilterComparator.notEqual => query.Where(t => t.Net != decimalFilterValue),
						TransactionContractFilterComparator.contains => query.Where(t => t.Net > decimalFilterValue),
						TransactionContractFilterComparator.doesNotContain => query.Where(t => t.Net < decimalFilterValue),
						_ => query,
					} : query;
				case SimpleTransactionContractProperty.TaxPercentage:
					return validDecimal ? filterComparator switch
					{
						TransactionContractFilterComparator.greaterThan => query.Where(t => t.Tax > decimalFilterValue),
						TransactionContractFilterComparator.lessThan => query.Where(t => t.Tax < decimalFilterValue),
						TransactionContractFilterComparator.greaterThanOrEqual => query.Where(t => t.Tax >= decimalFilterValue),
						TransactionContractFilterComparator.lessThanOrEqual => query.Where(t => t.Tax <= decimalFilterValue),
						TransactionContractFilterComparator.equal => query.Where(t => t.Tax == decimalFilterValue),
						TransactionContractFilterComparator.notEqual => query.Where(t => t.Tax != decimalFilterValue),
						TransactionContractFilterComparator.contains => query.Where(t => t.Tax > decimalFilterValue),
						TransactionContractFilterComparator.doesNotContain => query.Where(t => t.Tax < decimalFilterValue),
						_ => query,
					} : query;
				case SimpleTransactionContractProperty.TaxValue:
					return validDecimal ? filterComparator switch
					{
						TransactionContractFilterComparator.greaterThan => query.Where(t => t.Gross - t.Net > decimalFilterValue),
						TransactionContractFilterComparator.lessThan => query.Where(t => t.Gross - t.Net < decimalFilterValue),
						TransactionContractFilterComparator.greaterThanOrEqual => query.Where(t => t.Gross - t.Net >= decimalFilterValue),
						TransactionContractFilterComparator.lessThanOrEqual => query.Where(t => t.Gross - t.Net <= decimalFilterValue),
						TransactionContractFilterComparator.equal => query.Where(t => t.Gross - t.Net == decimalFilterValue),
						TransactionContractFilterComparator.notEqual => query.Where(t => t.Gross - t.Net != decimalFilterValue),
						TransactionContractFilterComparator.contains => query.Where(t => t.Gross - t.Net > decimalFilterValue),
						TransactionContractFilterComparator.doesNotContain => query.Where(t => t.Gross - t.Net < decimalFilterValue),
						_ => query,
					} : query;
				case SimpleTransactionContractProperty.Tags:
					return filterComparator switch
					{
						TransactionContractFilterComparator.equal => query.Where(t => t.Tags.Select(tag => tag.Name).SequenceEqual(tags)),
						TransactionContractFilterComparator.notEqual => query.Where(t => !t.Tags.Select(tag => tag.Name).SequenceEqual(tags)),
						TransactionContractFilterComparator.contains => query.Where(t => tags.All(text => t.Tags.Select(tag => tag.Name).Contains(text))),
						TransactionContractFilterComparator.doesNotContain => query.Where(t => !tags.All(text => t.Tags.Select(tag => tag.Name).Contains(text))),
						_ => query,
					};
				case SimpleTransactionContractProperty.TagsCount:
					return filterComparator switch
					{
						TransactionContractFilterComparator.greaterThan => query.Where(t => t.Tags.Count > tags.Length),
						TransactionContractFilterComparator.lessThan => query.Where(t => t.Tags.Count < tags.Length),
						TransactionContractFilterComparator.greaterThanOrEqual => query.Where(t => t.Tags.Count >= tags.Length),
						TransactionContractFilterComparator.lessThanOrEqual => query.Where(t => t.Tags.Count <= tags.Length),
						TransactionContractFilterComparator.equal => query.Where(t => t.Tags.Count == tags.Length),
						TransactionContractFilterComparator.notEqual => query.Where(t => t.Tags.Count != tags.Length),
						TransactionContractFilterComparator.contains => query.Where(t => t.Tags.Count > tags.Length),
						TransactionContractFilterComparator.doesNotContain => query.Where(t => t.Tags.Count < tags.Length),
						_ => query,
					};
				case SimpleTransactionContractProperty.CurrencyName:
					return filterComparator switch
					{
						TransactionContractFilterComparator.greaterThan => query.Where(t => t.Currency.Name.ToLower().CompareTo(filterValue.ToLower()) > 0),
						TransactionContractFilterComparator.lessThan => query.Where(t => t.Currency.Name.ToLower().CompareTo(filterValue.ToLower()) < 0),
						TransactionContractFilterComparator.greaterThanOrEqual => query.Where(t => t.Currency.Name.ToLower().CompareTo(filterValue.ToLower()) >= 0),
						TransactionContractFilterComparator.lessThanOrEqual => query.Where(t => t.Currency.Name.ToLower().CompareTo(filterValue.ToLower()) <= 0),
						TransactionContractFilterComparator.equal => query.Where(t => t.Currency.Name.ToLower().Equals(filterValue.ToLower())),
						TransactionContractFilterComparator.notEqual => query.Where(t => !t.Currency.Name.ToLower().Equals(filterValue.ToLower())),
						TransactionContractFilterComparator.contains => query.Where(t => t.Currency.Name.ToLower().Contains(filterValue.ToLower())),
						TransactionContractFilterComparator.doesNotContain => query.Where(t => !t.Currency.Name.ToLower().Contains(filterValue.ToLower())),
						_ => query,
					};
				case SimpleTransactionContractProperty.CurrencyTax:
					return validDecimal ? filterComparator switch
					{
						TransactionContractFilterComparator.greaterThan => query.Where(t => t.Currency.IngameTax > decimalFilterValue),
						TransactionContractFilterComparator.lessThan => query.Where(t => t.Currency.IngameTax < decimalFilterValue),
						TransactionContractFilterComparator.greaterThanOrEqual => query.Where(t => t.Currency.IngameTax >= decimalFilterValue),
						TransactionContractFilterComparator.lessThanOrEqual => query.Where(t => t.Currency.IngameTax <= decimalFilterValue),
						TransactionContractFilterComparator.equal => query.Where(t => t.Currency.IngameTax == decimalFilterValue),
						TransactionContractFilterComparator.notEqual => query.Where(t => t.Currency.IngameTax != decimalFilterValue),
						TransactionContractFilterComparator.contains => query.Where(t => t.Currency.IngameTax > decimalFilterValue),
						TransactionContractFilterComparator.doesNotContain => query.Where(t => t.Currency.IngameTax < decimalFilterValue),
						_ => query,
					} : query;
				case SimpleTransactionContractProperty.CurrencyStatus:
					return validBool ? filterComparator switch
					{
						TransactionContractFilterComparator.equal => query.Where(t => t.Currency.IsActive == boolFilterValue),
						TransactionContractFilterComparator.notEqual => query.Where(t => t.Currency.IsActive != boolFilterValue),
						_ => query,
					} : query;
				default:
					return query;
			}
		}
		public static IQueryable<SourceTransactionContractEntity> FilterSourceTransactions(this IQueryable<SourceTransactionContractEntity> query, IAccountEntity currentAccount, AdvancedTransactionContractProperty filterProperty, TransactionContractFilterComparator filterComparator, String filterValue)
		{
			if (Enum.IsDefined(typeof(SimpleTransactionContractProperty), (Int32)filterProperty))
			{
				return query.FilterTransactions<SourceTransactionContractEntity, AccountEntityBase, AccountEntityBase, AccountEntityBase, AccountEntityBase>((SimpleTransactionContractProperty)(Int32)filterProperty, filterComparator, filterValue);
			}
			filterValue ??= String.Empty;
			Boolean validDecimal = Decimal.TryParse(filterValue, out Decimal decimalFilterValue);
			if (validDecimal)
			{
				switch (filterProperty)
				{
					case AdvancedTransactionContractProperty.CreditorBookingDecimal:
						switch (filterComparator)
						{
							case TransactionContractFilterComparator.greaterThan:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.CreditorBookings.Sum(b => b.Value))) > decimalFilterValue);
							case TransactionContractFilterComparator.lessThan:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.CreditorBookings.Sum(b => b.Value))) < decimalFilterValue);
							case TransactionContractFilterComparator.greaterThanOrEqual:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.CreditorBookings.Sum(b => b.Value))) >= decimalFilterValue);
							case TransactionContractFilterComparator.lessThanOrEqual:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.CreditorBookings.Sum(b => b.Value))) <= decimalFilterValue);
							case TransactionContractFilterComparator.equal:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.CreditorBookings.Sum(b => b.Value))) == decimalFilterValue);
							case TransactionContractFilterComparator.notEqual:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.CreditorBookings.Sum(b => b.Value))) != decimalFilterValue);
							case TransactionContractFilterComparator.contains:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.CreditorBookings.Sum(b => b.Value))) > decimalFilterValue);
							case TransactionContractFilterComparator.doesNotContain:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.CreditorBookings.Sum(b => b.Value))) < decimalFilterValue);
							default:
								return query;
						}
					case AdvancedTransactionContractProperty.DebtorBookingDecimal:
						switch (filterComparator)
						{
							case TransactionContractFilterComparator.greaterThan:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.DebtorBookings.Sum(b => b.Value))) > decimalFilterValue);
							case TransactionContractFilterComparator.lessThan:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.DebtorBookings.Sum(b => b.Value))) < decimalFilterValue);
							case TransactionContractFilterComparator.greaterThanOrEqual:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.DebtorBookings.Sum(b => b.Value))) >= decimalFilterValue);
							case TransactionContractFilterComparator.lessThanOrEqual:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.DebtorBookings.Sum(b => b.Value))) <= decimalFilterValue);
							case TransactionContractFilterComparator.equal:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.DebtorBookings.Sum(b => b.Value))) == decimalFilterValue);
							case TransactionContractFilterComparator.notEqual:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.DebtorBookings.Sum(b => b.Value))) != decimalFilterValue);
							case TransactionContractFilterComparator.contains:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.DebtorBookings.Sum(b => b.Value))) > decimalFilterValue);
							case TransactionContractFilterComparator.doesNotContain:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.DebtorBookings.Sum(b => b.Value))) < decimalFilterValue);
							default:
								return query;
						}
					case AdvancedTransactionContractProperty.AccountBookingDecimal:
						switch (filterComparator)
						{
							case TransactionContractFilterComparator.greaterThan:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value))) > decimalFilterValue);
							case TransactionContractFilterComparator.lessThan:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value))) < decimalFilterValue);
							case TransactionContractFilterComparator.greaterThanOrEqual:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value))) >= decimalFilterValue);
							case TransactionContractFilterComparator.lessThanOrEqual:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value))) <= decimalFilterValue);
							case TransactionContractFilterComparator.equal:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value))) == decimalFilterValue);
							case TransactionContractFilterComparator.notEqual:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value))) != decimalFilterValue);
							case TransactionContractFilterComparator.contains:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value))) > decimalFilterValue);
							case TransactionContractFilterComparator.doesNotContain:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value))) < decimalFilterValue);
							default:
								return query;
						}
					case AdvancedTransactionContractProperty.CreditorBookingPercent:
						switch (filterComparator)
						{
							case TransactionContractFilterComparator.greaterThan:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.CreditorBookings.Sum(b => b.Value))) / s.Gross > decimalFilterValue);
							case TransactionContractFilterComparator.lessThan:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.CreditorBookings.Sum(b => b.Value))) / s.Gross < decimalFilterValue);
							case TransactionContractFilterComparator.greaterThanOrEqual:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.CreditorBookings.Sum(b => b.Value))) / s.Gross >= decimalFilterValue);
							case TransactionContractFilterComparator.lessThanOrEqual:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.CreditorBookings.Sum(b => b.Value))) / s.Gross <= decimalFilterValue);
							case TransactionContractFilterComparator.equal:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.CreditorBookings.Sum(b => b.Value))) / s.Gross == decimalFilterValue);
							case TransactionContractFilterComparator.notEqual:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.CreditorBookings.Sum(b => b.Value))) / s.Gross != decimalFilterValue);
							case TransactionContractFilterComparator.contains:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.CreditorBookings.Sum(b => b.Value))) / s.Gross > decimalFilterValue);
							case TransactionContractFilterComparator.doesNotContain:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.CreditorBookings.Sum(b => b.Value))) / s.Gross < decimalFilterValue);
							default:
								return query;
						}
					case AdvancedTransactionContractProperty.DebtorBookingPercent:
						switch (filterComparator)
						{
							case TransactionContractFilterComparator.greaterThan:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.DebtorBookings.Sum(b => b.Value))) / s.Gross > decimalFilterValue);
							case TransactionContractFilterComparator.lessThan:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.DebtorBookings.Sum(b => b.Value))) / s.Gross < decimalFilterValue);
							case TransactionContractFilterComparator.greaterThanOrEqual:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.DebtorBookings.Sum(b => b.Value))) / s.Gross >= decimalFilterValue);
							case TransactionContractFilterComparator.lessThanOrEqual:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.DebtorBookings.Sum(b => b.Value))) / s.Gross <= decimalFilterValue);
							case TransactionContractFilterComparator.equal:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.DebtorBookings.Sum(b => b.Value))) / s.Gross == decimalFilterValue);
							case TransactionContractFilterComparator.notEqual:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.DebtorBookings.Sum(b => b.Value))) / s.Gross != decimalFilterValue);
							case TransactionContractFilterComparator.contains:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.DebtorBookings.Sum(b => b.Value))) / s.Gross > decimalFilterValue);
							case TransactionContractFilterComparator.doesNotContain:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => t.DebtorBookings.Sum(b => b.Value))) / s.Gross < decimalFilterValue);
							default:
								return query;
						}
					case AdvancedTransactionContractProperty.AccountBookingPercent:
						switch (filterComparator)
						{
							case TransactionContractFilterComparator.greaterThan:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value))) / s.Gross > decimalFilterValue);
							case TransactionContractFilterComparator.lessThan:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value))) / s.Gross < decimalFilterValue);
							case TransactionContractFilterComparator.greaterThanOrEqual:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value))) / s.Gross >= decimalFilterValue);
							case TransactionContractFilterComparator.lessThanOrEqual:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value))) / s.Gross <= decimalFilterValue);
							case TransactionContractFilterComparator.equal:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value))) / s.Gross == decimalFilterValue);
							case TransactionContractFilterComparator.notEqual:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value))) / s.Gross != decimalFilterValue);
							case TransactionContractFilterComparator.contains:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value))) / s.Gross > decimalFilterValue);
							case TransactionContractFilterComparator.doesNotContain:
								return query.Where(s => (s.IsBooked ?
							s.Net :
							s.TargetTransactionContracts
								.Where(t => s.Relationship == TransactionPartnersRelationship.RealToReal ?
									t.Relationship == TransactionPartnersRelationship.RealToReal :
									s.Relationship == TransactionPartnersRelationship.RealToVirtual ?
									t.Relationship == TransactionPartnersRelationship.RealToForward :
									s.Relationship == TransactionPartnersRelationship.VirtualToReal ?
									t.Relationship == TransactionPartnersRelationship.ForwardToReal :
									t.Relationship == TransactionPartnersRelationship.ForwardToForward)
								.Sum(t => (t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value))) / s.Gross < decimalFilterValue);
							default:
								return query;
						}
				}
			}
			return query;
		}
		public static IQueryable<TargetTransactionContractEntity> FilterTargetTransactions(this IQueryable<TargetTransactionContractEntity> query, IAccountEntity currentAccount, AdvancedTransactionContractProperty filterProperty, TransactionContractFilterComparator filterComparator, String filterValue)
		{
			if (Enum.IsDefined(typeof(SimpleTransactionContractProperty), (Int32)filterProperty))
			{
				return query.FilterTransactions<TargetTransactionContractEntity, RealAccountEntity, RealAccountEntity, AccountEntityBase, AccountEntityBase>((SimpleTransactionContractProperty)(Int32)filterProperty, filterComparator, filterValue);
			}
			filterValue ??= String.Empty;
			Boolean validDecimal = Decimal.TryParse(filterValue, out Decimal decimalFilterValue);
			if (validDecimal)
			{
				switch (filterProperty)
				{
					case AdvancedTransactionContractProperty.CreditorBookingDecimal:
						switch (filterComparator)
						{
							case TransactionContractFilterComparator.greaterThan:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.CreditorBookings.Sum(b => b.Value)) > decimalFilterValue);
							case TransactionContractFilterComparator.lessThan:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.CreditorBookings.Sum(b => b.Value)) < decimalFilterValue);
							case TransactionContractFilterComparator.greaterThanOrEqual:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.CreditorBookings.Sum(b => b.Value)) >= decimalFilterValue);
							case TransactionContractFilterComparator.lessThanOrEqual:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.CreditorBookings.Sum(b => b.Value)) <= decimalFilterValue);
							case TransactionContractFilterComparator.equal:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.CreditorBookings.Sum(b => b.Value)) == decimalFilterValue);
							case TransactionContractFilterComparator.notEqual:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.CreditorBookings.Sum(b => b.Value)) != decimalFilterValue);
							case TransactionContractFilterComparator.contains:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.CreditorBookings.Sum(b => b.Value)) > decimalFilterValue);
							case TransactionContractFilterComparator.doesNotContain:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.CreditorBookings.Sum(b => b.Value)) < decimalFilterValue);
							default:
								return query;
						}
					case AdvancedTransactionContractProperty.DebtorBookingDecimal:
						switch (filterComparator)
						{
							case TransactionContractFilterComparator.greaterThan:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.DebtorBookings.Sum(b => b.Value)) > decimalFilterValue);
							case TransactionContractFilterComparator.lessThan:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.DebtorBookings.Sum(b => b.Value)) < decimalFilterValue);
							case TransactionContractFilterComparator.greaterThanOrEqual:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.DebtorBookings.Sum(b => b.Value)) >= decimalFilterValue);
							case TransactionContractFilterComparator.lessThanOrEqual:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.DebtorBookings.Sum(b => b.Value)) <= decimalFilterValue);
							case TransactionContractFilterComparator.equal:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.DebtorBookings.Sum(b => b.Value)) == decimalFilterValue);
							case TransactionContractFilterComparator.notEqual:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.DebtorBookings.Sum(b => b.Value)) != decimalFilterValue);
							case TransactionContractFilterComparator.contains:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.DebtorBookings.Sum(b => b.Value)) > decimalFilterValue);
							case TransactionContractFilterComparator.doesNotContain:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.DebtorBookings.Sum(b => b.Value)) < decimalFilterValue);
							default:
								return query;
						}
					case AdvancedTransactionContractProperty.AccountBookingDecimal:
						switch (filterComparator)
						{
							case TransactionContractFilterComparator.greaterThan:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							(t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value)) > decimalFilterValue);
							case TransactionContractFilterComparator.lessThan:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							(t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value)) < decimalFilterValue);
							case TransactionContractFilterComparator.greaterThanOrEqual:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							(t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value)) >= decimalFilterValue);
							case TransactionContractFilterComparator.lessThanOrEqual:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							(t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value)) <= decimalFilterValue);
							case TransactionContractFilterComparator.equal:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							(t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value)) == decimalFilterValue);
							case TransactionContractFilterComparator.notEqual:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							(t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value)) != decimalFilterValue);
							case TransactionContractFilterComparator.contains:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							(t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value)) > decimalFilterValue);
							case TransactionContractFilterComparator.doesNotContain:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							(t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value)) < decimalFilterValue);
							default:
								return query;
						}
					case AdvancedTransactionContractProperty.CreditorBookingPercent:
						switch (filterComparator)
						{
							case TransactionContractFilterComparator.greaterThan:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.CreditorBookings.Sum(b => b.Value)) / t.Gross > decimalFilterValue);
							case TransactionContractFilterComparator.lessThan:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.CreditorBookings.Sum(b => b.Value)) / t.Gross < decimalFilterValue);
							case TransactionContractFilterComparator.greaterThanOrEqual:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.CreditorBookings.Sum(b => b.Value)) / t.Gross >= decimalFilterValue);
							case TransactionContractFilterComparator.lessThanOrEqual:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.CreditorBookings.Sum(b => b.Value)) / t.Gross <= decimalFilterValue);
							case TransactionContractFilterComparator.equal:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.CreditorBookings.Sum(b => b.Value)) / t.Gross == decimalFilterValue);
							case TransactionContractFilterComparator.notEqual:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.CreditorBookings.Sum(b => b.Value)) / t.Gross != decimalFilterValue);
							case TransactionContractFilterComparator.contains:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.CreditorBookings.Sum(b => b.Value)) / t.Gross > decimalFilterValue);
							case TransactionContractFilterComparator.doesNotContain:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.CreditorBookings.Sum(b => b.Value)) / t.Gross < decimalFilterValue);
							default:
								return query;
						}
					case AdvancedTransactionContractProperty.DebtorBookingPercent:
						switch (filterComparator)
						{
							case TransactionContractFilterComparator.greaterThan:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.DebtorBookings.Sum(b => b.Value)) / t.Gross > decimalFilterValue);
							case TransactionContractFilterComparator.lessThan:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.DebtorBookings.Sum(b => b.Value)) / t.Gross < decimalFilterValue);
							case TransactionContractFilterComparator.greaterThanOrEqual:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.DebtorBookings.Sum(b => b.Value)) / t.Gross >= decimalFilterValue);
							case TransactionContractFilterComparator.lessThanOrEqual:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.DebtorBookings.Sum(b => b.Value)) / t.Gross <= decimalFilterValue);
							case TransactionContractFilterComparator.equal:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.DebtorBookings.Sum(b => b.Value)) / t.Gross == decimalFilterValue);
							case TransactionContractFilterComparator.notEqual:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.DebtorBookings.Sum(b => b.Value)) / t.Gross != decimalFilterValue);
							case TransactionContractFilterComparator.contains:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.DebtorBookings.Sum(b => b.Value)) / t.Gross > decimalFilterValue);
							case TransactionContractFilterComparator.doesNotContain:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							t.DebtorBookings.Sum(b => b.Value)) / t.Gross < decimalFilterValue);
							default:
								return query;
						}
					case AdvancedTransactionContractProperty.AccountBookingPercent:
						switch (filterComparator)
						{
							case TransactionContractFilterComparator.greaterThan:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							(t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value)) / t.Gross > decimalFilterValue);
							case TransactionContractFilterComparator.lessThan:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							(t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value)) / t.Gross < decimalFilterValue);
							case TransactionContractFilterComparator.greaterThanOrEqual:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							(t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value)) / t.Gross >= decimalFilterValue);
							case TransactionContractFilterComparator.lessThanOrEqual:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							(t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value)) / t.Gross <= decimalFilterValue);
							case TransactionContractFilterComparator.equal:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							(t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value)) / t.Gross == decimalFilterValue);
							case TransactionContractFilterComparator.notEqual:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							(t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value)) / t.Gross != decimalFilterValue);
							case TransactionContractFilterComparator.contains:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							(t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value)) / t.Gross > decimalFilterValue);
							case TransactionContractFilterComparator.doesNotContain:
								return query.Where(t => (t.IsBooked ?
							t.Net :
							(t.Creditor.Id == currentAccount.Id ? t.CreditorBookings : t.DebtorBookings).Sum(b => b.Value)) / t.Gross < decimalFilterValue);
							default:
								return query;
						}
				}
			}
			return query;
		}
	}
}
