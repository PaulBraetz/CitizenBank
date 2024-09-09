namespace CBCommon.Enums
{
    public static class CitizenBankEnums
	{
		public enum AccountType : int
		{
			None,
			Real,
			Virtual,
			Department
		}
		public enum DefaultTimeSpanDays : int
		{
			Long = 28,
			Medium = 14,
			Short = 3
		}
		public enum CreateTransactionOfferRequestType : int
		{
			Regular,
			Custom
		}
		public enum PerPageValue : int
		{
			One = 1,
			Five = 5,
			Ten = 10,
			Twentyfive = 25,
			Fifty = 50,
			Onehundred = 100
		}
		public enum AdvancedTransactionContractProperty : int
		{
			None = SimpleTransactionContractProperty.None,
			Gross = SimpleTransactionContractProperty.Gross,
			Net = SimpleTransactionContractProperty.Net,
			TaxValue = SimpleTransactionContractProperty.TaxValue,
			TaxPercentage = SimpleTransactionContractProperty.TaxPercentage,
			Created = SimpleTransactionContractProperty.Created,
			Due = SimpleTransactionContractProperty.ExpirationDate,
			CreditorName = SimpleTransactionContractProperty.CreditorName,
			DebtorName = SimpleTransactionContractProperty.DebtorName,
			Usage = SimpleTransactionContractProperty.Usage,
			Tags = SimpleTransactionContractProperty.Tags,
			TagsCount = SimpleTransactionContractProperty.TagsCount,
			CurrencyName = SimpleTransactionContractProperty.CurrencyName,
			CurrencyTax = SimpleTransactionContractProperty.CurrencyTax,
			CurrencyStatus = SimpleTransactionContractProperty.CurrencyStatus,
			IsBooked,
			CreditorBookingDecimal,
			DebtorBookingDecimal,
			AccountBookingDecimal,
			CreditorBookingPercent,
			DebtorBookingPercent,
			AccountBookingPercent
		}
		public enum SimpleTransactionContractProperty : int
		{
			None,
			Gross,
			Net,
			TaxValue,
			TaxPercentage,
			Created,
			ExpirationDate,
			CreditorName,
			DebtorName,
			CreatorName,
			RecipientName,
			Usage,
			Tags,
			TagsCount,
			CurrencyName,
			CurrencyTax,
			CurrencyStatus
		}
		public static List<SimpleTransactionContractProperty> SpecialTransactionContractFilterProperties { get; } = new()
		{
			SimpleTransactionContractProperty.Tags,
			SimpleTransactionContractProperty.TagsCount,
			SimpleTransactionContractProperty.CurrencyStatus
		};
		public static List<SimpleTransactionContractProperty> DecimalTransactionContractFilterProperties { get; } = new()
		{
			SimpleTransactionContractProperty.Gross,
			SimpleTransactionContractProperty.Net,
			SimpleTransactionContractProperty.TaxPercentage,
			SimpleTransactionContractProperty.TaxValue,
			SimpleTransactionContractProperty.CurrencyTax
		};
		public static List<SimpleTransactionContractProperty> DateTimeTransactionContractFilterProperties { get; } = new()
		{
			SimpleTransactionContractProperty.Created,
			SimpleTransactionContractProperty.ExpirationDate
		};
		public static List<SimpleTransactionContractProperty> StringTransactionContractFilterProperties { get; } = new()
		{
			SimpleTransactionContractProperty.CreditorName,
			SimpleTransactionContractProperty.DebtorName,
			SimpleTransactionContractProperty.Usage,
			SimpleTransactionContractProperty.CurrencyName
		};
		public enum TransactionContractFilterComparator : int
		{
			None,
			greaterThan,
			lessThan,
			greaterThanOrEqual,
			lessThanOrEqual,
			equal,
			notEqual,
			contains,
			doesNotContain
		}
		public enum TransactionOfferAnswer : int
		{
			None,
			Accepted,
			Rejected
		}
		public enum AccountQueryType : int
		{
			All,
			Real,
			Virtual,
			DepositToReferenceAccount,
			ForwardToReferenceAccount
		}
		public enum TransactionPartnersRelationship : int
		{
			None,
			Complex,
			Real,
			RealToVirtual,
			VirtualToVirtual,
			RealToReal,
			VirtualToReal,
			RealToForward,
			ForwardToReal,
			ForwardToDeposit,
			DepositToForward,
			ForwardToForward,
			EqualizingDepositToForward,
			EqualizingForwardToDeposit,
			Equalizing
		}
		public static ICollection<TransactionPartnersRelationship> FirstTargetRelationships => new List<TransactionPartnersRelationship>
			{
				TransactionPartnersRelationship.RealToReal,
				TransactionPartnersRelationship.RealToForward,
				TransactionPartnersRelationship.DepositToForward
			};
		public static List<TransactionPartnersRelationship> ComplexTransactionPartnersRelationships => new()
		{
			TransactionPartnersRelationship.Complex,
			TransactionPartnersRelationship.RealToVirtual,
			TransactionPartnersRelationship.VirtualToReal,
			TransactionPartnersRelationship.VirtualToVirtual,
			TransactionPartnersRelationship.Equalizing
		};
		public static List<TransactionPartnersRelationship> RealTransactionPartnersRelationships => new()
		{
			TransactionPartnersRelationship.Real,
			TransactionPartnersRelationship.RealToReal,
			TransactionPartnersRelationship.ForwardToReal,
			TransactionPartnersRelationship.RealToForward,
			TransactionPartnersRelationship.ForwardToDeposit,
			TransactionPartnersRelationship.DepositToForward,
			TransactionPartnersRelationship.ForwardToForward,
			TransactionPartnersRelationship.EqualizingDepositToForward,
			TransactionPartnersRelationship.EqualizingForwardToDeposit
		};
	}
}
