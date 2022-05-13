using CBApplication.Services.Abstractions;

using CBData.Entities;

using CBFrontend.UI.DataFrames.Requests.Children;

using PBApplication.Requests;

using System;
using System.Threading.Tasks;

using static CBApplication.Services.Abstractions.ICitizenService;

namespace CitizenBank.Pages.Citizen.Components.Requests
{
	public partial class List : CitizenRequestsChild
	{
		private async Task Verify(CitizenLinkRequestEntity request)
		{
			await SessionParent.ServiceContext
					   .GetService<ICitizenService>()
					   .VerifyCitizenLinkRequest(new AsUserEncryptableRequest<VerifyCitizenLinkRequestParameter>()
					   {
						   Parameter = new VerifyCitizenLinkRequestParameter()
						   {
							   RequestId = request.Id
						   },
						   AsUserId = SessionParent.Session.User.Id
					   });
		}
		private async Task Delete(Guid requestId)
		{
			await SessionParent
					.ServiceContext
					.GetService<ICitizenService>()
					.CancelCitizenLinkRequest(
					   new AsUserEncryptableRequest<IEventfulCitizenService.CancelCitizenLinkRequestParameter>()
					   {
						   Parameter = new CancelCitizenLinkRequestParameter()
						   {
							   RequestId = requestId
						   },
						   AsUserId = SessionParent.Session.User.Id
					   });
		}
	}
}
