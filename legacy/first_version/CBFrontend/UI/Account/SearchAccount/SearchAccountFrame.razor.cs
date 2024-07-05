using CBApplication.Requests;
using CBApplication.Services.Abstractions;
using CBData.Abstractions;

using Microsoft.AspNetCore.Components;
using PBApplication.Responses;
using PBApplication.Responses.Abstractions;

using PBFrontend.UI.Authorization;

using System.Threading.Tasks;

namespace CBFrontend.UI.Account.SearchAccount
{
	public partial class SearchAccountFrame : SessionChild
	{
		[Parameter]
		public RenderFragment ChildContent { get; set; }

		[Parameter]
		public IAccountEntity Value { get; set; }
		[Parameter]
		public EventCallback<IAccountEntity> ValueChanged { get; set; }

		public AsAccountGetPaginatedEncryptableRequest<IAccountService.SearchAccountsParameterBase> Request = new()
		{
			PerPage = 5,
			Parameter = new()
			{
				Accessibility = PBCommon.Enums.AccessibilityType.Public
			},
		};
		public IGetPaginatedResponse<IAccountEntity> Response = new GetPaginatedResponse<IAccountEntity>();
		public async Task Submit()
		{
			Response = await SessionParent.ServiceContext.GetService<IAccountService>().SearchAccounts(Request);
		}

		public async Task Clear()
		{
			await Select(null);
		}

		public async Task Select(IAccountEntity account)
		{
			Request.Parameter.Name = account?.Name;
			Response = new GetPaginatedResponse<IAccountEntity>();
			await ValueChanged.InvokeAsync(Value = account);
			await InvokeAsync(StateHasChanged);
		}
	}
}
