using CBApplication.Requests;

using CBData.Entities;

using PBApplication.Events;
using PBApplication.Extensions;
using PBApplication.Requests;
using PBApplication.Responses;
using PBApplication.Services.Abstractions;
using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;
using PBCommon.Globalization;

using PBData.Entities;

using System;
using System.Threading.Tasks;

namespace CBApplication.Services.Abstractions
{
	public interface IEventfulCitizenService : ICitizenService, IEventfulService
	{
		//Payload : new request
		//Recipients : affected user
		event ServiceEventHandler<ServiceEventArgs<CitizenLinkRequestEntity>> OnCitizenLinkRequestCreated;

		//Recipients : affected request
		event ServiceEventHandler<ServiceEventArgs> OnCitizenLinkRequestCancelled;

		//Recipients : affected request
		event ServiceEventHandler<ServiceEventArgs> OnCitizenLinkRequestVerified;
		//Payload : new citizen		
		//Recipients : affected owner, affected citizen
		event ServiceEventHandler<ServiceEventArgs<CitizenEntity>> OnCitizenLinked;

		//Payload : new settings
		//Recipients : affected settings
		event ServiceEventHandler<ServiceEventArgs<CitizenSettingsEntity>> OnCitizenSettingsChanged;

		//Payload : unlinked citizen, previous owner key, current owner key, time when event was raised
		//Recipients : affected citizen
		sealed class OnCitizenUnlinkedData : EncryptableBase<Guid>
		{
			public OnCitizenUnlinkedData() { }
			public OnCitizenUnlinkedData(CitizenEntity citizen, UserEntity previousOwner, UserEntity currentOwner)
			{
				RaisedAt = TimeManager.Now;
				PreviousOwner = previousOwner;
				CurrentOwner = currentOwner;
				Citizen = citizen;
			}
			public DateTimeOffset RaisedAt { get; set; }
			public UserEntity PreviousOwner { get; set; }
			public UserEntity CurrentOwner { get; set; }
			public CitizenEntity Citizen { get; set; }

			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				await Task.WhenAll(
					PreviousOwner.SafeDecrypt(decryptor),
					CurrentOwner.SafeDecrypt(decryptor),
					Citizen.SafeDecrypt(decryptor));
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				await Task.WhenAll(
					PreviousOwner.SafeEncrypt(encryptor),
					CurrentOwner.SafeEncrypt(encryptor),
					Citizen.SafeEncrypt(encryptor));
			}
		}
		event ServiceEventHandler<ServiceEventArgs<OnCitizenUnlinkedData>> OnCitizenUnlinked;

		Task<CitizenEntity> GetCitizen(String name);
		Task EnsureSettingsAndAccountForCitizen(CitizenEntity citizen);
	}
}
