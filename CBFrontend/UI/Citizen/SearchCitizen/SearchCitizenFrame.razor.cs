using CBApplication.Services.Abstractions;

using CBData.Entities;

using Microsoft.AspNetCore.Components;

using PBApplication.Requests;
using PBApplication.Requests.Abstractions;
using PBApplication.Responses;
using PBApplication.Responses.Abstractions;

using PBFrontend.UI.Authorization;

using System.Threading.Tasks;

namespace CBFrontend.UI.Citizen.SearchCitizen
{
	public partial class SearchCitizenFrame : SessionChild
	{
		[Parameter]
		public RenderFragment ChildContent { get; set; }

		[Parameter]
		public CitizenEntity Value { get; set; }
		[Parameter]
		public EventCallback<CitizenEntity> ValueChanged { get; set; }

		public IGetPaginatedAsUserEncryptableRequest<ICitizenService.SearchCitizensParameter> Request = new GetPaginatedAsUserEncryptableRequest<ICitizenService.SearchCitizensParameter>()
		{
			PerPage = 5,
			Parameter = new()
		};
		public IGetPaginatedResponse<CitizenEntity> Response = new GetPaginatedResponse<CitizenEntity>();
		public async Task Submit()
		{
			Response = await SessionParent.ServiceContext.GetService<ICitizenService>().SearchCitizens(Request);
		}

		public async Task Clear()
		{
			await Select(new());
		}

		public async Task Select(CitizenEntity citizen)
		{
			Request.Parameter.Name = citizen.Name;
			Response = new GetPaginatedResponse<CitizenEntity>();
			await ValueChanged.InvokeAsync(Value = citizen);
			await InvokeAsync(StateHasChanged);
		}
	}
}
