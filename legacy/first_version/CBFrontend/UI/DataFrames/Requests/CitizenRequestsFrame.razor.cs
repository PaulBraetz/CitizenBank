using CBApplication.Services.Abstractions;

using CBData.Entities;

namespace CBFrontend.UI.DataFrames.Requests
{
    public partial class CitizenRequestsFrame : SessionChild
	{
		[Parameter]
		public RenderFragment ChildContent { get; set; }

		public IReadOnlyList<CitizenLinkRequestEntity> Requests => requests.AsReadOnly();
		private List<CitizenLinkRequestEntity> requests = new();

		protected override async Task OnParametersSetAndSessionInitializedAsync()
		{
			var response = await SessionParent.ServiceContext.GetService<IEventfulCitizenService>().GetCitizenLinkRequests();
			if (response.HasData())
			{
				requests = response.Data.ToList();
				foreach (var request in requests)
				{
					await Subscribe(new EventSubscription(nameof(IEventfulCitizenService.OnCitizenLinkRequestVerified), request.HubId), () => remove(request));
					await Subscribe(new EventSubscription(nameof(IEventfulCitizenService.OnCitizenLinkRequestCancelled), request.HubId), () => remove(request));
				}
				await Subscribe<CitizenLinkRequestEntity>(new EventSubscription(nameof(IEventfulCitizenService.OnCitizenLinkRequestCreated), SessionParent.Session.User.HubId), add);

				void add(CitizenLinkRequestEntity response)
				{
					requests.Add(response);
					Subscribe(new EventSubscription(nameof(IEventfulCitizenService.OnCitizenLinkRequestVerified), response.HubId), () => remove(response));
					Subscribe(new EventSubscription(nameof(IEventfulCitizenService.OnCitizenLinkRequestCancelled), response.HubId), () => remove(response));
					InvokeAsync(StateHasChanged);
				}
				void remove(CitizenLinkRequestEntity request)
				{
					requests.Remove(request);
					Unsubscribe(new EventSubscription(nameof(IEventfulCitizenService.OnCitizenLinkRequestVerified), request.HubId));
					Unsubscribe(new EventSubscription(nameof(IEventfulCitizenService.OnCitizenLinkRequestCancelled), request.HubId));
					InvokeAsync(StateHasChanged);
				}
			}
		}
	}
}
