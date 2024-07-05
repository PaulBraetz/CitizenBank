using CBApplication.Services.Abstractions;

using Microsoft.AspNetCore.SignalR;

using PBApplication.Services.Abstractions;
using PBCommon.Serialization;

using PBServer.Context.Observers;
using PBServer.Hubs;

namespace CitizenBank.Events.Observers
{
	internal sealed class CitizenServiceObserver : ServiceBaseObserver<IEventfulCitizenService>
	{
		public CitizenServiceObserver(IHubContext<EventHub> hubContext, IEncryptionService encryptionService, ISerializer serializer) : base(hubContext, encryptionService, serializer)
		{
		}

		public override void Observe(IEventfulCitizenService service)
		{
			base.Observe(service);
			service.OnCitizenLinkRequestCancelled += async (args) => await Send(nameof(service.OnCitizenLinkRequestCancelled), args);
			service.OnCitizenLinkRequestCreated += async (args) => await SendGuidEncryptable(nameof(service.OnCitizenLinkRequestCreated), args);
			service.OnCitizenLinkRequestVerified += async (args) => await Send(nameof(service.OnCitizenLinkRequestVerified), args);
			service.OnCitizenSettingsChanged += async (args) => await SendGuidEncryptable(nameof(service.OnCitizenSettingsChanged), args);
			service.OnUserAccessedByDelegate += async (args) => await SendGuidEncryptable(nameof(service.OnUserAccessedByDelegate), args);
			service.OnCitizenUnlinked += async (args) => await SendGuidEncryptable(nameof(service.OnCitizenUnlinked), args);
			service.OnCitizenLinked += async (args) => await SendGuidEncryptable(nameof(service.OnCitizenLinked), args);
		}
	}
}
