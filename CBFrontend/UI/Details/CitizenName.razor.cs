using CBApplication.Services.Abstractions;
using CBData.Entities;

using Microsoft.AspNetCore.Components;
using PBApplication.Requests;
using PBApplication.Services.Abstractions;
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

		private Boolean isVerified = false;

		private CitizenEntity previousCitizen;

		protected override async Task OnParametersSetAsync()
		{
			if (Citizen != null && Citizen != previousCitizen) {
				var response = await SessionParent.ServiceContext.GetService<IClaimService>().GetClaims(new AsUserGetPaginatedEncryptableRequest<IClaimService.GetClaimsParameter>()
				{
					AsUserId = SessionParent.Session.User.Id,
					Parameter = new()
					{
						ValueId = Citizen.Id
					}
				});
				isVerified = response.Data.Any(c => c.Rights.Contains(PBCommon.Configuration.Settings.OWNER_RIGHT));
				previousCitizen = Citizen;
				await base.OnParametersSetAsync();
			}
		}

		protected override async Task OnParametersSetAndSessionInitializedAsync()
		{
			await SubscribeOnce<IEventfulCitizenService.OnCitizenUnlinkedData>(new EventSubscription(nameof(IEventfulCitizenService.OnCitizenUnlinked), Citizen.HubId), onUnlinked);
			await SubscribeOnce<CitizenEntity>(new EventSubscription(nameof(IEventfulCitizenService.OnCitizenLinked), Citizen.HubId), onLinked);

			void onUnlinked(IEventfulCitizenService.OnCitizenUnlinkedData data)
			{
				Citizen = data.Citizen;
				isVerified = data.CurrentOwner is not null;
				InvokeAsync(StateHasChanged);
			}
			void onLinked(CitizenEntity citizen)
			{
				Citizen = citizen;
				isVerified = true;
				InvokeAsync(StateHasChanged);
			}
		}
	}
}
