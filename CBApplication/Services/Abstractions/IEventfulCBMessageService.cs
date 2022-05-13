
using CBData.Entities;

using PBApplication.Events;
using PBApplication.Services.Abstractions;
using PBCommon.Globalization;
using System.Collections.Generic;

namespace CBApplication.Services.Abstractions
{
	public interface IEventfulCBMessageService : ICBMessageService, IEventfulService
	{
		//Payload : message
		//Recipients : creator, recipients
		event ServiceEventHandler<ServiceEventArgs<AccountMessageEntity>> OnAccountMessageCreated;
		void CreateAccountMessage(AccountEntityBase creator, AccountEntityBase recipient, LocalizableFormattableString message);
		void CreateAccountMessages(AccountEntityBase creator, IEnumerable<AccountEntityBase> recipients, LocalizableFormattableString message);
		void CreateAccountSelfMessage(AccountEntityBase creator, LocalizableFormattableString message);
		void CreateAccountSelfMessages(IEnumerable<AccountEntityBase> creators, LocalizableFormattableString message);
		//Payload : message
		//Recipients : creator, recipients
		event ServiceEventHandler<ServiceEventArgs<CitizenMessageEntity>> OnCitizenMessageCreated;
		void CreateCitizenMessage(CitizenEntity creator, CitizenEntity recipient, LocalizableFormattableString message);
		void CreateCitizenMessages(CitizenEntity creator, IEnumerable<CitizenEntity> recipients, LocalizableFormattableString message);
		void CreateCitizenSelfMessage(CitizenEntity creator, LocalizableFormattableString message);
		void CreateCitizenSelfMessages(IEnumerable<CitizenEntity> creators, LocalizableFormattableString message);
	}
}
