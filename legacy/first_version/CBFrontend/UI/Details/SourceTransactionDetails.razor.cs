using CBData.Entities;

namespace CBFrontend.UI.Details
{
    partial class SourceTransactionDetails : SessionChild
	{
		[Parameter]
		public SourceTransactionContractEntity Transaction { get; set; }

		public override String ToString()
		{
			var v0 = Transaction.Debtor.Name;
			var v1 = Transaction.Gross;
			var v2 = v1 > 1 ? Transaction.Currency.PluralName : Transaction.Currency.Name;
			var v3 = Transaction.Net;
			var v4 = v3 > 1 ? Transaction.Currency.PluralName : Transaction.Currency.Name;
			var v5 = Transaction.Creditor.Name;
			var v6 = Transaction.LifeSpan.Days;
			var localizedString = SessionParent.Localize("{0} is sending {1:0}{2} ({3:0}{4}) to {5}, due in {6} days.");
			return String.Format(localizedString, v0, v1, v2, v3, v4, v5, v6);
		}
	}
}
