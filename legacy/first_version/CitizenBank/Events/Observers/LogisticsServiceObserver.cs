using CBApplication.Services.Abstractions;

using Microsoft.AspNetCore.SignalR;

using PBApplication.Services.Abstractions;
using PBCommon.Serialization;

using PBServer.Context.Observers;
using PBServer.Hubs;

namespace CitizenBank.Events.Observers
{
	public sealed class LogisticsServiceObserver : ServiceBaseObserver<IEventfulLogisticsService>
	{
		public LogisticsServiceObserver(IHubContext<EventHub> hubContext, IEncryptionService encryptionService, ISerializer serializer) : base(hubContext, encryptionService, serializer)
		{
		}

		public override void Observe(IEventfulLogisticsService service)
		{
			base.Observe(service);
			service.OnLogisticsOrderCreated += (args) => SendGuidEncryptable(nameof(IEventfulLogisticsService.OnLogisticsOrderCreated), args);
			service.OnLogisticsOrderDeleted += (args) => Send(nameof(IEventfulLogisticsService.OnLogisticsOrderDeleted), args);
			service.OnLogisticsOrderEdited += (args) => SendGuidEncryptable(nameof(IEventfulLogisticsService.OnLogisticsOrderEdited), args);
		}
	}
}
