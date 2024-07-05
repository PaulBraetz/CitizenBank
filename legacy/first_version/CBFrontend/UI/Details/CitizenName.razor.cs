using CBApplication.Services.Abstractions;
using CBData.Entities;

using Microsoft.AspNetCore.Components;
using PBFrontend.UI.Authorization;
using PBShared.Events;
using System.Threading.Tasks;

namespace CBFrontend.UI.Details
{
	partial class CitizenName : SessionChild
	{
		[Parameter]
		public CitizenEntity Citizen { get; set; }

		protected override async Task OnParametersSetAndSessionInitializedAsync()
		{
			await Subscribe<IEventfulCitizenService.OnCitizenUnlinkedData>(new EventSubscription(nameof(IEventfulCitizenService.OnCitizenUnlinked), Citizen.HubId), onUnlinked);
			await Subscribe<CitizenEntity>(new EventSubscription(nameof(IEventfulCitizenService.OnCitizenLinked), Citizen.HubId), onLinked);

			void onUnlinked(IEventfulCitizenService.OnCitizenUnlinkedData data)
			{
				Citizen = data.Citizen;
				InvokeAsync(StateHasChanged);
			}
			void onLinked(CitizenEntity citizen)
			{
				Citizen = citizen;
				InvokeAsync(StateHasChanged);
			}
		}
	}
}
