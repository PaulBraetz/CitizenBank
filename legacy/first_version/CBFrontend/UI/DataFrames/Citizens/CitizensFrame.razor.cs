using CBApplication.Services.Abstractions;

using CBData.Entities;

using Microsoft.AspNetCore.Components;

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

		private List<CitizenEntity> citizens = new();
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
			if (previousUser == null || (SessionParent.Session.IsLoggedIn && previousUser.Id != SessionParent.Session.User.Id))
			{
				if (previousUser != null)
				{
					await generalUnsubscribe();
				}

				previousUser = SessionParent.Session.User;

				citizens = (await SessionParent.ServiceContext.GetService<IEventfulCitizenService>().GetCitizens()).Data.ToList();
				await Task.WhenAll(citizens.Select(c => new EventSubscription(nameof(IEventfulCitizenService.OnCitizenUnlinked), c.HubId)).Select(s => Subscribe<IEventfulCitizenService.OnCitizenUnlinkedData>(s, remove)));
				await Subscribe<CitizenEntity>(new EventSubscription(nameof(IEventfulCitizenService.OnCitizenLinked), SessionParent.Session.User.HubId), add);

				async Task generalUnsubscribe()
				{
					await Unsubscribe(citizens.Select(c => new EventSubscription(nameof(IEventfulCitizenService.OnCitizenUnlinked), c.HubId)));
					await Unsubscribe(new EventSubscription(nameof(IEventfulCitizenService.OnCitizenLinked), previousUser.HubId));
				}
				void add(CitizenEntity response)
				{
					citizens.Add(response);
					Subscribe<IEventfulCitizenService.OnCitizenUnlinkedData>(new EventSubscription(nameof(IEventfulCitizenService.OnCitizenUnlinked), response.HubId), remove);
					InvokeAsync(StateHasChanged);
				}
				void remove(IEventfulCitizenService.OnCitizenUnlinkedData data)
				{
					citizens.Remove(citizens.SingleOrDefault(c => c.Id == data.Citizen.Id));
					Unsubscribe(new EventSubscription(nameof(IEventfulCitizenService.OnCitizenUnlinked), data.Citizen.HubId));
					InvokeAsync(StateHasChanged);
				}
			}
		}
	}
}
