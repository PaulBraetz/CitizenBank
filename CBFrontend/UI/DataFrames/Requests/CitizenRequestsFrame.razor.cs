using CBApplication.Services.Abstractions;

using CBData.Entities;

using Microsoft.AspNetCore.Components;
using PBFrontend.UI.Authorization;
using PBShared.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBFrontend.UI.DataFrames.Requests
{
	public partial class CitizenRequestsFrame : SessionChild
	{
		[Parameter]
		public RenderFragment ChildContent { get; set; }


		public IReadOnlyList<CitizenLinkRequestEntity> Requests => requests.AsReadOnly();
		private List<CitizenLinkRequestEntity> requests { get; set; } = new();

		protected override async Task OnParametersSetAndSessionInitializedAsync()
		{
			requests = (await SessionParent.ServiceContext.GetService<ICitizenService>().GetCitizenLinkRequests()).Data.ToList();
			foreach (var request in requests)
			{
				await SubscribeOnce(new EventSubscription(nameof(ICitizenService.OnCitizenLinkRequestVerified), request.HubId), () => remove(request));
				await SubscribeOnce(new EventSubscription(nameof(ICitizenService.OnCitizenLinkRequestCancelled), request.HubId), () => remove(request));
			}
			await SubscribeOnce<CitizenLinkRequestEntity>(new EventSubscription(nameof(ICitizenService.OnCitizenLinkRequestCreated), SessionParent.Session.Owner.HubId), add);

			void add(CitizenLinkRequestEntity response)
			{
				requests.Add(response);
				SubscribeOnce(new EventSubscription(nameof(ICitizenService.OnCitizenLinkRequestVerified), response.HubId), () => remove(response));
				SubscribeOnce(new EventSubscription(nameof(ICitizenService.OnCitizenLinkRequestCancelled), response.HubId), () => remove(response));
				InvokeAsync(StateHasChanged);
			}
			void remove(CitizenLinkRequestEntity request)
			{
				requests.Remove(request);
				Unsubscribe(new EventSubscription(nameof(ICitizenService.OnCitizenLinkRequestVerified), request.HubId));
				Unsubscribe(new EventSubscription(nameof(ICitizenService.OnCitizenLinkRequestCancelled), request.HubId));
				InvokeAsync(StateHasChanged);
			}
		}
	}
}
