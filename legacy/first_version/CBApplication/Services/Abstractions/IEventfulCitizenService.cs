
using CBData.Entities;

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
			public OnCitizenUnlinkedData(CitizenEntity citizen, UserEntity previousOwner)
			{
				RaisedAt = TimeManager.Now;
				PreviousOwner = previousOwner;
				Citizen = citizen;
			}
			public DateTimeOffset RaisedAt { get; set; }
			public UserEntity PreviousOwner { get; set; }
			public CitizenEntity Citizen { get; set; }

			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				await Task.WhenAll(
					PreviousOwner.SafeDecrypt(decryptor),
					Citizen.SafeDecrypt(decryptor));
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				await Task.WhenAll(
					PreviousOwner.SafeEncrypt(encryptor),
					Citizen.SafeEncrypt(encryptor));
			}
		}
		event ServiceEventHandler<ServiceEventArgs<OnCitizenUnlinkedData>> OnCitizenUnlinked;

		Task<CitizenEntity> GetCitizen(String name);
		Task EnsureSettingsAndAccountForCitizen(CitizenEntity citizen);
	}
}
