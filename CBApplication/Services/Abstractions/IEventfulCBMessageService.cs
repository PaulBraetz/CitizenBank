
using CBApplication.Requests;

using CBData.Entities;

using PBApplication.Events;
using PBApplication.Responses;
using PBApplication.Services.Abstractions;
using PBData.Entities;

using System;
using System.Collections.Generic;

namespace CBApplication.Services.Abstractions
{
	public interface IEventfulCBMessageService : ICBMessageService, IEventfulService
	{
		//Payload : message
		//Recipients : creator, recipients
		event ServiceEventHandler<ServiceEventArgs<AccountMessageEntity>> OnAccountMessageCreated;
		void CreateAccountMessage(AccountEntityBase creator, AccountEntityBase recipient, String message);
		void CreateAccountMessages(AccountEntityBase creator, ICollection<AccountEntityBase> recipients, String message);
		void CreateAccountSelfMessage(AccountEntityBase creator, String message);
		void CreateAccountSelfMessages(ICollection<AccountEntityBase> creators, String message);
		//Payload : message
		//Recipients : creator, recipients
		event ServiceEventHandler<ServiceEventArgs<CitizenMessageEntity>> OnCitizenMessageCreated;
		void CreateCitizenMessage(CitizenEntity creator, CitizenEntity recipient, String message);
		void CreateCitizenMessages(CitizenEntity creator, ICollection<CitizenEntity> recipients, String message);
		void CreateCitizenSelfMessage(CitizenEntity creator, String message);
		void CreateCitizenSelfMessages(ICollection<CitizenEntity> creators, String message);
	}
}
