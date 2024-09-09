using CBData.Entities;

namespace CBFrontend.UI.Details
{
    partial class TransactionOfferDetails : SessionChild
	{
		[Parameter]
		public TransactionOfferEntity Offer { get; set; }
		public override String ToString()
		{
			var v0 = Offer.Debtor.Name;
			var v1 = Offer.Gross;
			var v2 = v1 > 1 ? Offer.Currency.PluralName : Offer.Currency.Name;
			var v3 = Offer.Net;
			var v4 = v3 > 1 ? Offer.Currency.PluralName : Offer.Currency.Name;
			var v5 = Offer.Creditor.Name;
			var v6 = Offer.LifeSpan.Days;
			var localizedString = SessionParent.Localize("{0} is offering {1:0}{2} ({3:0}{4}) to {5}, due in {6} days.");
			return String.Format(localizedString, v0, v1, v2, v3, v4, v5, v6);
		}
	}
}
