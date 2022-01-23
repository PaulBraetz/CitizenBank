using CBApplication.Services.Abstractions;

using CBData.Entities;

using CBFrontend.UI.DataFrames.Requests.Children;

using CitizenBank.Pages.Banking;

using PBApplication.Requests;
using PBApplication.Services.Abstractions;

using PBFrontend.UI.Miscellaneous.Loading;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static CBApplication.Services.Abstractions.ICitizenServiceBase;

namespace CitizenBank.Pages.Citizen.Components.Requests
{
	public partial class List : CitizenRequestsChild
	{
		private async Task Verify(CitizenLinkRequestEntity request)
		{
			await SessionParent.ServiceContext
					   .GetService<ICitizenServiceBase>()
					   .VerifyCitizenLinkRequest(new AsUserEncryptableRequest<VerifyCitizenLinkRequestParameter>()
					   {
						   Parameter = new VerifyCitizenLinkRequestParameter()
						   {
							   RequestId = request.Id
						   },
						   AsUserId = SessionParent.Session.Owner.Id
					   });
		}
		private async Task Delete(Guid requestId)
		{
			await SessionParent
					.ServiceContext
					.GetService<ICitizenServiceBase>()
					.CancelCitizenLinkRequest(
					   new AsUserEncryptableRequest<ICitizenService.CancelCitizenLinkRequestParameter>()
					   {
						   Parameter = new CancelCitizenLinkRequestParameter()
						   {
							   RequestId = requestId
						   },
						   AsUserId = SessionParent.Session.Owner.Id
					   });
		}
	}
}
