using CBApplication.Services.Abstractions;

using CBData.Entities;

using Microsoft.AspNetCore.Components;

using PBApplication.Extensions;

using PBData.Entities;
using PBFrontend.UI.Authorization;
using PBShared.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBFrontend.UI.DataFrames.Citizens
{
	public partial class CitizensFrame : SessionChild
	{
		private CitizenEntity currentCitizen;

		[Parameter]
		public RenderFragment ChildContent { get; set; }

		public IReadOnlyList<CitizenEntity> Citizens => citizens.AsReadOnly();

		private List<CitizenEntity> citizens = new List<CitizenEntity>();
		public CitizenEntity CurrentCitizen
		{
			get => currentCitizen ??= Citizens.FirstOrDefault();
			set
			{
				currentCitizen = value;
				InvokeAsync(StateHasChanged);
			}
		}

		private UserEntity previousUser;

		protected override async Task OnParametersSetAndSessionInitializedAsync()
		{
			if (previousUser == null || previousUser.Id != SessionParent.Session.Owner.Id)
			{
				if (previousUser != null)
				{
					await generalUnsubscribe();
				}

				previousUser = SessionParent.Session.Owner;

				citizens = (await SessionParent.ServiceContext.GetService<ICitizenService>().GetCitizens()).Data.ToList();
				await Task.WhenAll(citizens.Select(c => new EventSubscription(nameof(ICitizenService.OnCitizenUnlinked), c.HubId)).Select(s => SubscribeOnce<ICitizenService.OnCitizenUnlinkedData>(s, remove)));
				await SubscribeOnce<CitizenEntity>(new EventSubscription(nameof(ICitizenService.OnCitizenLinked), SessionParent.Session.Owner.HubId), add);

				async Task generalUnsubscribe()
				{
					await Unsubscribe(citizens.Select(c => new EventSubscription(nameof(ICitizenService.OnCitizenUnlinked), c.HubId)));
					await Unsubscribe(new EventSubscription(nameof(ICitizenService.OnCitizenLinked), previousUser.HubId));
				}
				void add(CitizenEntity response)
				{
					citizens.Add(response);
					SubscribeOnce<ICitizenService.OnCitizenUnlinkedData>(new EventSubscription(nameof(ICitizenService.OnCitizenUnlinked), response.HubId), remove);
					InvokeAsync(StateHasChanged);
				}
				void remove(ICitizenService.OnCitizenUnlinkedData data)
				{
					citizens.Remove(citizens.SingleOrDefault(c => c.Id == data.Citizen.Id));
					Unsubscribe(new EventSubscription(nameof(ICitizenService.OnCitizenUnlinked), data.Citizen.HubId));
					InvokeAsync(StateHasChanged);
				}
			}
		}
	}
}
