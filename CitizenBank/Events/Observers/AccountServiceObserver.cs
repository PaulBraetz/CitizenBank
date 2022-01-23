using CBApplication.Services.Abstractions;

using Microsoft.AspNetCore.SignalR;

using PBApplication.Services.Abstractions;
using PBCommon.Serialization;

using PBServer.Context.Observers;
using PBServer.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitizenBank.Events.Observers
{
	public sealed class AccountServiceObserver : ServiceBaseObserver<IEventfulAccountService>
	{
		public AccountServiceObserver(IHubContext<EventHub> hubContext, IEncryptionService encryptionService, ISerializer serializer) : base(hubContext, encryptionService, serializer)
		{
		}

		public override void Observe(IEventfulAccountService service)
		{
			base.Observe(service);
			service.OnAdminRecruitedForAccount += async (args) => await SendGuidEncryptable(nameof(IEventfulAccountService.OnAdminRecruitedForAccount), args);
			service.OnAdminRecruitedForAdmin += async (args) => await SendGuidEncryptable(nameof(IEventfulAccountService.OnAdminRecruitedForAdmin), args);
			service.OnAdminResignedForAccount += async (args) => await SendGuidEncryptable(nameof(IEventfulAccountService.OnAdminResignedForAccount), args);
			service.OnAdminResignedForAdmin += async (args) => await SendGuidEncryptable(nameof(IEventfulAccountService.OnAdminResignedForAdmin), args);
			service.OnDepartmentAccountCreated += async (args) => await SendGuidEncryptable(nameof(IEventfulAccountService.OnDepartmentAccountCreated), args);
			service.OnDepartmentAccountSettingsChanged += async (args) => await SendGuidEncryptable(nameof(IEventfulAccountService.OnDepartmentAccountSettingsChanged), args);
			service.OnDepositAccountReferenceChangedForReferenced += async (args) => await SendGuidEncryptable(nameof(IEventfulAccountService.OnDepositAccountReferenceChangedForReferenced), args);
			service.OnDepositAccountReferenceChangedForReferencing += async (args) => await SendGuidEncryptable(nameof(IEventfulAccountService.OnDepositAccountReferenceChangedForReferencing), args);
			service.OnDepositAccountReferenceCreatedForReferenced += async (args) => await SendGuidEncryptable(nameof(IEventfulAccountService.OnDepositAccountReferenceCreatedForReferenced), args);
			service.OnDepositAccountReferenceCreatedForReferencing += async (args) => await SendGuidEncryptable(nameof(IEventfulAccountService.OnDepositAccountReferenceCreatedForReferencing), args);
			service.OnDepositAccountReferenceDeleted += async (args) => await Send(nameof(IEventfulAccountService.OnDepositAccountReferenceDeleted), args);
			service.OnRealAccountSettingsChanged += async (args) => await SendGuidEncryptable(nameof(IEventfulAccountService.OnRealAccountSettingsChanged), args);
			service.OnUserAccessedByDelegate += async (args) => await SendGuidEncryptable(nameof(IEventfulAccountService.OnUserAccessedByDelegate), args);
			service.OnVirtualAccountCreated += async (args) => await SendGuidEncryptable(nameof(IEventfulAccountService.OnVirtualAccountCreated), args);
			service.OnVirtualAccountSettingsChanged += async (args) => await SendGuidEncryptable(nameof(IEventfulAccountService.OnVirtualAccountSettingsChanged), args);
		}
	}
}
