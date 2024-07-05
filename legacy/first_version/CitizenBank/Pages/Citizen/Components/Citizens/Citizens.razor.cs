using CBApplication.Services.Abstractions;

using CBData.Entities;

using CBFrontend.UI.DataFrames.Citizens.Children;

namespace CitizenBank.Pages.Citizen.Components.Citizens
{
	public partial class Citizens : CitizensFrameChild
	{
		private void Unlink(CitizenEntity response)
		{
			SessionParent.ServiceContext.GetService<IEventfulCitizenService>()
				.UnlinkCitizen(new CBApplication.Requests.AsCitizenRequest()
				{
					AsCitizenId = response.Id,
					AsUserId = SessionParent.Session.User.Id
				});
		}
	}
}
