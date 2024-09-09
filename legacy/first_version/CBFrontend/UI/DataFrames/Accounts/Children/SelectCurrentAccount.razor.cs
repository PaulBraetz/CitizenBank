
using CBData.Abstractions;
using CBData.Entities;

namespace CBFrontend.UI.DataFrames.Accounts.Children
{
    partial class SelectCurrentAccount : AccountsFrameChild
	{
		private IEnumerable<SelectInput<Guid>.OptionModel> options;

		private Guid Id
		{
			get => AccountsParent.CurrentAccount.Id;
			set => AccountsParent.CurrentAccount = AccountsParent.VirtualAccounts.SingleOrDefault(a => a.Id == value) as IAccountEntity ??
				AccountsParent.RealAccounts.SingleOrDefault(a => a.Id == value) as IAccountEntity;
		}

		private CitizenEntity previousCitizen;

		protected override async Task OnParametersSetAndSessionInitializedAsync()
		{
			await base.OnParametersSetAndSessionInitializedAsync();
			if (previousCitizen == null || previousCitizen.Id != CitizensParent.CurrentCitizen.Id)
			{
				previousCitizen = CitizensParent.CurrentCitizen;
				options = AccountsParent.RealAccounts.Cast<IAccountEntity>().Concat(AccountsParent.VirtualAccounts.Cast<IAccountEntity>())
					.Select(a => new SelectInput<Guid>.OptionModel(a.Id, a.Name, false));
			}
		}
	}
}
