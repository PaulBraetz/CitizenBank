using CBApplication.Requests;
using CBApplication.Services.Abstractions;

using CBData.Abstractions;
using CBData.Entities;

using CBFrontend.UI.DataFrames.Citizens.Children;

using Microsoft.AspNetCore.Components;

using PBApplication.Extensions;
using PBApplication.Services.Abstractions;
using PBFrontend.UI.Authorization;
using PBShared.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBFrontend.UI.DataFrames.Accounts
{
	public partial class AccountsFrame : CitizensFrameChild
	{
		[Parameter]
		public RenderFragment ChildContent { get; set; }

		public IEnumerable<VirtualAccountEntity> VirtualAccounts => virtualAccounts.AsReadOnly();
		public IEnumerable<DepartmentAccountEntity> DepartmentAccounts => departmentAccounts.AsReadOnly();
		public IEnumerable<RealAccountEntity> RealAccounts => realAccounts.AsReadOnly();

		private List<VirtualAccountEntity> virtualAccounts = new();
		private List<DepartmentAccountEntity> departmentAccounts = new();
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
						AsUserId = SessionParent.Session.Owner.Id
					})).Data;

				virtualAccounts = accounts.OfType<VirtualAccountEntity>().ToList();

				departmentAccounts = accounts.OfType<DepartmentAccountEntity>().ToList();
				//await Task.WhenAll(departmentAccounts.Select(subscribeToDepartmentAccountResignation));

				realAccounts = accounts.OfType<RealAccountEntity>().ToList();

				currentAccount = realAccounts.First();

				await generalSubscribe();

				async Task generalSubscribe()
				{
					await SubscribeOnce<VirtualAccountEntity>(new EventSubscription(nameof(IEventfulAccountService.OnAdminRecruitedForAdmin), CitizensParent.CurrentCitizen.HubId), addVirtualAccount);
					await SubscribeOnce<VirtualAccountEntity>(new EventSubscription(nameof(IEventfulAccountService.OnAdminResignedForAdmin), CitizensParent.CurrentCitizen.HubId), removeVirtualAccount);
					await SubscribeOnce<VirtualAccountEntity>(new EventSubscription(nameof(IEventfulAccountService.OnVirtualAccountCreated), CitizensParent.CurrentCitizen.HubId), addVirtualAccount);

					await SubscribeOnce<DepartmentAccountEntity>(new EventSubscription(nameof(IEventfulAccountService.OnDepartmentAccountCreated), CitizensParent.CurrentCitizen.HubId), addDepartmentAccount);
				}

				async Task generalUnsubscribe()
				{
					await Unsubscribe(new EventSubscription(nameof(IEventfulAccountService.OnAdminRecruitedForAdmin), previousCitizen.HubId));
					await Unsubscribe(new EventSubscription(nameof(IEventfulAccountService.OnAdminResignedForAdmin), previousCitizen.HubId));
					await Unsubscribe(new EventSubscription(nameof(IEventfulAccountService.OnAdminRecruitedForAdmin), previousCitizen.HubId));

					await Unsubscribe(new EventSubscription(nameof(IEventfulAccountService.OnDepartmentAccountCreated), previousCitizen.HubId));
				}

				void addVirtualAccount(VirtualAccountEntity account)
				{
					virtualAccounts.Add(account);
					InvokeAsync(StateHasChanged);
				}
				void removeVirtualAccount(VirtualAccountEntity account)
				{
					account = virtualAccounts.SingleOrDefault(a => a.Id == account.Id);
					if (CitizensParent.CurrentCitizen.Id != account.Owner.Id)
					{
						virtualAccounts.Remove(account);
					}
					else
					{
						account.Admins.Remove(account.Admins.SingleOrDefault(a => a.Id == CitizensParent.CurrentCitizen.Id));
					}
					InvokeAsync(StateHasChanged);
				}

				void addDepartmentAccount(DepartmentAccountEntity account)
				{
					departmentAccounts.Add(account);

					//subscribeToVirtualAccountResignation(account);

					InvokeAsync(StateHasChanged);
				}
				async Task subscribeToDepartmentAccountEvents(DepartmentAccountEntity account)
				{
					await SubscribeOnce(new EventSubscription(nameof(IDepartmentService.OnDepartmentDeleted), account.Department.HubId), () => removeDepartment(account));
				}
				void removeDepartment(DepartmentAccountEntity account)
				{
					departmentAccounts.Remove(departmentAccounts.SingleOrDefault(a => a.Id == account.Id));
					InvokeAsync(StateHasChanged);
				}
			}
		}
	}
}
