using CBApplication.Requests;
using CBApplication.Requests.Abstractions;
using CBApplication.Services.Abstractions;

using CBData.Entities;

using CBFrontend.UI.DataFrames.Accounts.Children;

using static CBCommon.Enums.CitizenBankEnums;

namespace CBFrontend.UI.Transactions
{
    partial class TransactionOffers : AccountsFrameChild
	{
		private IGetPaginatedEncryptableResponse<TransactionOfferEntity> response;
		private IAsAccountGetPaginatedRequest<ITransactionService.GetTransactionOffersParameter> request;

		protected override async Task OnParametersSetAndSessionInitializedAsync()
		{
			await Refresh();
			await base.OnParametersSetAndSessionInitializedAsync();
		}

		private LoadingFrame refreshLoadingFrameRef;

		private async Task Refresh()
		{
			request = new AsAccountGetPaginatedRequest<ITransactionService.GetTransactionOffersParameter>()
			{
				AsUserId = SessionParent.Session.User.Id,
				AsAccountId = AccountsParent.CurrentAccount.Id,
				AsCitizenId = CitizensParent.CurrentCitizen.Id,
				Page = 0,
				PerPage = 100,
				Parameter = new ITransactionService.GetTransactionOffersParameter()
			};
			response = await SessionParent.ServiceContext.GetService<ITransactionService>().GetTransactionOffers(request);
			await InvokeAsync(StateHasChanged);
		}

		private async Task Accept(TransactionOfferEntity offer) => await Answer(offer, TransactionOfferAnswer.Accepted);
		private async Task Reject(TransactionOfferEntity offer) => await Answer(offer, TransactionOfferAnswer.Rejected);
		private async Task Answer(TransactionOfferEntity offer, TransactionOfferAnswer answer)
		{
			var request = new AsAccountEncryptableRequest<ITransactionService.AnswerTransactionOfferParameter>()
			{
				AsUserId = SessionParent.Session.User.Id,
				AsAccountId = AccountsParent.CurrentAccount.Id,
				AsCitizenId = CitizensParent.CurrentCitizen.Id,
				Parameter = new ITransactionService.AnswerTransactionOfferParameter()
				{
					Answer = answer,
					TransactionOfferId = offer.Id
				}
			};
			await SessionParent.ServiceContext.GetService<ITransactionService>().AnswerTransactionOffer(request);
			await refreshLoadingFrameRef.Load(async () => { await Refresh(); });
		}
	}
}
