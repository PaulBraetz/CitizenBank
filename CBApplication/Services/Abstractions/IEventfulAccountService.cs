

using CBApplication.Requests;

using CBData.Abstractions;
using CBData.Entities;

using PBApplication.Events;
using PBApplication.Responses;
using PBApplication.Services.Abstractions;
using PBData.Abstractions;

using System;
using System.Linq;
using static CBApplication.Services.Abstractions.IEventfulAccountService;

namespace CBApplication.Services.Abstractions
{
	public interface IEventfulAccountService : IAccountService, IEventfulService
	{
		//Payload : admin
		//Recipients : affected account
		event ServiceEventHandler<ServiceEventArgs<CitizenEntity>> OnAdminResignedForAccount;
		//Payload : affected account
		//Recipients : admin
		event ServiceEventHandler<ServiceEventArgs<VirtualAccountEntity>> OnAdminResignedForAdmin;
		//Payload : new admin
		//Recipients : affected account
		event ServiceEventHandler<ServiceEventArgs<CitizenEntity>> OnAdminRecruitedForAccount;
		//Payload : affected account
		//Recipients : new admin
		event ServiceEventHandler<ServiceEventArgs<VirtualAccountEntity>> OnAdminRecruitedForAdmin;
		//Payload : new settings
		//Recipients : affected settings
		event ServiceEventHandler<ServiceEventArgs<RealAccountSettingsEntity>> OnRealAccountSettingsChanged;
		//Payload : new settings
		//Recipients : affected settings
		event ServiceEventHandler<ServiceEventArgs<VirtualAccountSettingsEntity>> OnVirtualAccountSettingsChanged;
		//Payload : affected reference (advanced details)
		//Recipients : referencing account
		event ServiceEventHandler<ServiceEventArgs<DepositAccountReferenceEntity>> OnDepositAccountReferenceChangedForReferencing;
		//Payload : affected reference (non advanced details)
		//Recipients : referenced account
		event ServiceEventHandler<ServiceEventArgs<DepositAccountReferenceEntity>> OnDepositAccountReferenceChangedForReferenced;
		//Recipients : affected reference
		event ServiceEventHandler<ServiceEventArgs> OnDepositAccountReferenceDeleted;
		//Payload : new account
		//Recipients : creator
		event ServiceEventHandler<ServiceEventArgs<VirtualAccountEntity>> OnVirtualAccountCreated;
		//Payload : new deposit reference (non advanced details)
		//Recipients : referenced
		event ServiceEventHandler<ServiceEventArgs<DepositAccountReferenceEntity>> OnDepositAccountReferenceCreatedForReferenced;
		//Payload : new deposit reference (advanced details)
		//Recipients : referencing
		event ServiceEventHandler<ServiceEventArgs<DepositAccountReferenceEntity>> OnDepositAccountReferenceCreatedForReferencing;
		//Payload : new balance
		//Recipients : affected deposit
		event ServiceEventHandler<ServiceEventArgs<Decimal>> OnDepositBalanceUpdated;

		VirtualAccountSettingsEntity UpdateVirtualAccountSettings(VirtualAccountEntity account);

		void UpdateDepositBalance(DepositAccountReferenceEntity depositAccountReference, Decimal value);
		void UpdateDepositBalance(VirtualAccountEntity account, RealAccountEntity mappedAccount, Decimal value);
	}
}
