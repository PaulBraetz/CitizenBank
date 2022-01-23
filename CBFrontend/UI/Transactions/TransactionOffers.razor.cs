using CBApplication.Requests;
using CBApplication.Requests.Abstractions;
using CBApplication.Services.Abstractions;
using CBData.Entities;
using CBFrontend.UI.DataFrames.Accounts.Children;
using PBApplication.Requests.Abstractions;
using PBApplication.Responses.Abstractions;
using PBFrontend.UI.Miscellaneous.Loading;
using PBShared.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CBCommon.Enums.CitizenBankEnums;

namespace CBFrontend.UI.Transactions
{
	partial class TransactionOffers : AccountsFrameChild
	{
		private IGetPaginatedEncryptableResponse<TransactionOfferEntity> response;
		private IGetPaginatedAsAccountRequest<ITransactionService.GetTransactionOffersParameter> request;

		protected override async Task OnParametersSetAndSessionInitializedAsync()
		{
			await Refresh();
			await base.OnParametersSetAndSessionInitializedAsync();
		}

		private LoadingFrame refreshLoadingFrameRef;

		private async Task Refresh()
		{
			request = new GetPaginatedAsAccountRequest<ITransactionService.GetTransactionOffersParameter>()
			{
				AsUserId = SessionParent.Session.Owner.Id,
				AsAccountId = AccountsParent.CurrentAccount.Id,
				AsCitizenId = (AccountsParent.CurrentAccount as RealAccountEntity).Owner.Id,
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
				AsUserId = SessionParent.Session.Owner.Id,
				AsAccountId = AccountsParent.CurrentAccount.Id,
				AsCitizenId = (AccountsParent.CurrentAccount as RealAccountEntity).Owner.Id,
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
