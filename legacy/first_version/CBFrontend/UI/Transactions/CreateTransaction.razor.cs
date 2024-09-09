using CBApplication.Requests;
using CBApplication.Requests.Abstractions;
using CBApplication.Services.Abstractions;

using CBData.Abstractions;
using CBData.Entities;

using CBFrontend.UI.DataFrames.Accounts.Children;

namespace CBFrontend.UI.Transactions
{
    partial class CreateTransaction : AccountsFrameChild
	{
		private Boolean creatorIsCreditor;
		private IAccountEntity debtor;
		private IAccountEntity creditor;

		private ITransactionService.CreateTransactionOfferParameter parameter;

		private ValidationFieldSet validation;

		private IResponse<SourceTransactionContractEntity> preview;

		private IEnumerable<SelectInput<Guid>.OptionModel> currencyOptions;

		protected override async Task OnParametersSetAndSessionInitializedAsync()
		{
			await base.OnParametersSetAndSessionInitializedAsync();
			var currencies = await SessionParent.ServiceContext.GetService<ITransactionService>().GetCurrencies(new GetPaginatedRequest<ITransactionService.GetCurrenciesParameter>() { Parameter = new() });
			if (currencies.Validation.AllValid)
			{
				currencyOptions = currencies.Data.Select(c => new SelectInput<Guid>.OptionModel(c.Id, c.Name)).ToArray();
			}
			parameter = new()
			{
				CurrencyId = currencyOptions.First().Value
			};
		}

		private IAsAccountEncryptableRequest<ITransactionService.CreateTransactionOfferParameter> GetRequest()
		{
			parameter.CreditorId = creditor?.Id ?? Guid.Empty;
			parameter.DebtorId = debtor?.Id ?? Guid.Empty;
			parameter.RecipientId = creatorIsCreditor ? parameter.DebtorId : parameter.CreditorId;

			var creator = creatorIsCreditor ? creditor : debtor;

			var request = new AsAccountEncryptableRequest<ITransactionService.CreateTransactionOfferParameter>()
			{
				Parameter = parameter,
				AsUserId = SessionParent.Session.User.Id,
				AsCitizenId = SessionParent.Session.User?.Id ?? Guid.Empty,
				AsAccountId = creator?.Id ?? Guid.Empty
			};

			return request;
		}
		private async Task Preview()
		{
			preview = await SessionParent.ServiceContext.GetService<ITransactionService>().GetTransactionSourcePreview(GetRequest());
			validation = preview.Validation;
		}
		private async Task Create()
		{
			var response = await SessionParent.ServiceContext.GetService<ITransactionService>().CreateTransactionOffer(GetRequest());
			validation = response.Validation;
			if (validation.NoneInvalid)
			{
				Reset();
			}
		}
		private void Reset()
		{
			validation = ValidationFieldSet.Empty;
			preview = null;
		}
	}
}
