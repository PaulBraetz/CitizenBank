
using CBApplication.Services.Abstractions;
using CBData.Entities;
using PBApplication.Requests;
using PBApplication.Requests.Abstractions;
using PBApplication.Responses;
using PBApplication.Responses.Abstractions;
using PBFrontend.UI.Authorization;
using PBFrontend.UI.Miscellaneous.Loading;
using PBShared.Events;
using System.Threading.Tasks;

namespace CBFrontend.UI.Logistics
{
	public partial class List : SessionChild
	{
		private IGetPaginatedResponse<LogisticsOrderEntity> orders = new GetPaginatedResponse<LogisticsOrderEntity>();
		private readonly IGetPaginatedEncryptableRequest<IEventfulLogisticsService.GetLogisticsOrdersParameter> request = new GetPaginatedEncryptableRequest<IEventfulLogisticsService.GetLogisticsOrdersParameter>()
		{
			PerPage = 100,
			Parameter = new ILogisticsService.GetLogisticsOrdersParameter()
		};

		protected override async Task OnParametersSetAndSessionInitializedAsync()
		{
			await Subscribe<LogisticsOrderEntity>(new EventSubscription(nameof(IEventfulLogisticsService.OnLogisticsOrderCreated), SessionParent.Session.HubId), a => Refresh());

			orders = await SessionParent.ServiceContext.GetService<IEventfulLogisticsService>().GetLogisticsOrders(request);
		}

		private LoadingFrame refreshLoadingFrameRef;

		private async Task Refresh()
		{
			orders = await SessionParent.ServiceContext.GetService<IEventfulLogisticsService>().GetLogisticsOrders(request);
			await InvokeAsync(StateHasChanged);
		}
	}
}
