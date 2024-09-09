using CBApplication.Requests;
using CBApplication.Services.Abstractions;

using CBData.Abstractions;
using CBData.Entities;

using CBFrontend.UI.DataFrames.Citizens.Children;

namespace CBFrontend.UI.DataFrames.Accounts
{
    public partial class AccountsFrame : CitizensFrameChild
	{
		[Parameter]
		public RenderFragment ChildContent { get; set; }

		public IEnumerable<VirtualAccountEntity> VirtualAccounts => virtualAccounts.AsReadOnly();
		public IEnumerable<RealAccountEntity> RealAccounts => realAccounts.AsReadOnly();

		private List<VirtualAccountEntity> virtualAccounts = new();
		private List<RealAccountEntity> realAccounts = new();

		private IAccountEntity currentAccount;
		public IAccountEntity CurrentAccount
		{
			get => currentAccount;
			set
			{
				currentAccount = value;
				InvokeAsync(StateHasChanged);
			}
		}

		private CitizenEntity previousCitizen;

		protected override async Task OnParametersSetAndSessionInitializedAsync()
		{
			await base.OnParametersSetAndSessionInitializedAsync();
			if (CitizensParent.CurrentCitizen != null && (previousCitizen == null || CitizensParent.CurrentCitizen.Id != previousCitizen.Id))
			{
				if (previousCitizen != null)
				{
					await generalUnsubscribe();
				}

				previousCitizen = CitizensParent.CurrentCitizen;

				var accounts = (await SessionParent.ServiceContext.GetService<IEventfulAccountService>()
					.GetAccounts(new AsCitizenRequest()
					{
						AsCitizenId = CitizensParent.CurrentCitizen.Id,
						AsUserId = SessionParent.Session.User.Id
					})).Data;

				virtualAccounts = accounts.OfType<VirtualAccountEntity>().ToList();

				//await Task.WhenAll(departmentAccounts.Select(subscribeToDepartmentAccountResignation));

				realAccounts = accounts.OfType<RealAccountEntity>().ToList();

				currentAccount = realAccounts.First();

				await generalSubscribe();

				async Task generalSubscribe()
				{
					await Subscribe<VirtualAccountEntity>(new EventSubscription(nameof(IEventfulAccountService.OnVirtualAccountCreated), CitizensParent.CurrentCitizen.HubId), addVirtualAccount);
				}

				async Task generalUnsubscribe()
				{
					await Task.CompletedTask;
				}

				void addVirtualAccount(VirtualAccountEntity account)
				{
					virtualAccounts.Add(account);
					InvokeAsync(StateHasChanged);
				}
				void removeVirtualAccount(VirtualAccountEntity account)
				{
					account = virtualAccounts.SingleOrDefault(a => a.Id == account.Id);
					//TODO: add owner claim check
					//if (CitizensParent.CurrentCitizen.Id != account.Owner.Id)
					//{
					//    virtualAccounts.Remove(account);
					//}
					InvokeAsync(StateHasChanged);
				}
			}
		}
	}
}
