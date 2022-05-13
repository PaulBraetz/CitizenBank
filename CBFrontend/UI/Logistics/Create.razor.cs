using CBApplication.Requests;
using CBApplication.Services.Abstractions;

using CBData.Entities;

using PBApplication.Responses;
using PBApplication.Responses.Abstractions;
using PBCommon.Globalization;
using PBFrontend.UI.Authorization;
using PBFrontend.UI.Miscellaneous.Loading;
using System;
using System.Threading.Tasks;

namespace CBFrontend.UI.Logistics
{
	public partial class Create : SessionChild
	{
		private AsCitizenRequest<ILogisticsService.CreateLogisticsOrderParameter> request = new()
		{
			Parameter = new()
			{
				Deadline = TimeManager.Now + TimeSpan.FromDays(1)
			}
		};
		private IResponse response = new Response();

		private LoadingFrame loadingFrameRef;

		private DateTimeOffset deadlineDate = TimeManager.Now + TimeSpan.FromDays(1);

		private DateTimeOffset deadlineTime;

		private CitizenEntity client = new();

		private DateTimeOffset TotalDeadline => deadlineDate.Date + deadlineTime.TimeOfDay;

		private async Task Submit()
		{
			async Task run()
			{
				request.Parameter.Deadline = TotalDeadline;
				request.AsCitizenId = client.Id;
				response = await SessionParent.ServiceContext.GetService<IEventfulLogisticsService>().CreateLogisticsOrder(request);
				if (response.Validation.NoneInvalid)
				{
					request = new()
					{
						Parameter = new()
						{
							Deadline = TimeManager.Now + TimeSpan.FromDays(1)
						}
					};
					deadlineDate = TimeManager.Now + TimeSpan.FromDays(1);
					deadlineTime = DateTimeOffset.MinValue;
					response = new Response();
				}
				await InvokeAsync(StateHasChanged);
			}
			await loadingFrameRef.Load(run);
		}
	}
}
