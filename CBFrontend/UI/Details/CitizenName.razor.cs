using CBApplication.Services.Abstractions;
using CBData.Entities;

using Microsoft.AspNetCore.Components;
using PBFrontend.UI.Authorization;
using PBShared.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBFrontend.UI.Details
{
	partial class CitizenName : SessionChild
	{
		[Parameter]
		public CitizenEntity Citizen { get; set; }

		protected override async Task OnParametersSetAndSessionInitializedAsync()
		{
			await SubscribeOnce<ICitizenService.OnCitizenUnlinkedData>(new EventSubscription(nameof(ICitizenService.OnCitizenUnlinked), Citizen.HubId), onUnlinked);
			await SubscribeOnce<CitizenEntity>(new EventSubscription(nameof(ICitizenService.OnCitizenLinked), Citizen.HubId), onLinked);

			void onUnlinked(ICitizenService.OnCitizenUnlinkedData data)
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
