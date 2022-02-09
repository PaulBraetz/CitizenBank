using CBApplication.Requests;
using CBApplication.Requests.Abstractions;

using CBData.Entities;

using PBApplication.Events;
using PBApplication.Extensions;
using PBApplication.Requests;
using PBApplication.Requests.Abstractions;
using PBApplication.Responses;
using PBApplication.Responses.Abstractions;
using PBApplication.Services.Abstractions;
using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;
using PBCommon.Globalization;

using PBData.Entities;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PBCommon.Enums;

namespace CBApplication.Services.Abstractions
{
	public interface ICitizenService : IService
	{
		sealed class CreateCitizenLinkRequestParameter
		{
			public String Name { get; set; }
		}
		Task<IResponse> CreateCitizenLinkRequest(IAsUserRequest<CreateCitizenLinkRequestParameter> request);

		sealed class CancelCitizenLinkRequestParameter : EncryptableBase<Guid>
		{
			public Guid RequestId { get; set; }

			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				RequestId = await decryptor.Decrypt(RequestId);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				RequestId = await encryptor.Encrypt(RequestId);
			}
		}
		Task<IResponse> CancelCitizenLinkRequest(IAsUserEncryptableRequest<CancelCitizenLinkRequestParameter> request);

		sealed class VerifyCitizenLinkRequestParameter : EncryptableBase<Guid>
		{
			public Guid RequestId { get; set; }
			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				RequestId = await decryptor.Decrypt(RequestId);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				RequestId = await encryptor.Encrypt(RequestId);
			}
		}
		Task<IResponse> VerifyCitizenLinkRequest(IAsUserEncryptableRequest<VerifyCitizenLinkRequestParameter> request);

		sealed class GetCitizenLinkRequestsParameter { }
		Task<IGetPaginatedEncryptableResponse<CitizenLinkRequestEntity>> GetCitizenLinkRequests(IAsUserGetPaginatedRequest<GetCitizenLinkRequestsParameter> request);
		Task<IGetPaginatedEncryptableResponse<CitizenLinkRequestEntity>> GetCitizenLinkRequests();

		sealed class GetCitizensParameter { }
		Task<IGetPaginatedEncryptableResponse<CitizenEntity>> GetCitizens(IAsUserGetPaginatedRequest<GetCitizensParameter> request);
		Task<IGetPaginatedEncryptableResponse<CitizenEntity>> GetCitizens();

		Task<IEncryptableResponse<CitizenSettingsEntity>> GetCitizenSettings(IAsCitizenRequest request);

		sealed class SetCitizenSettingsParameter
		{
			public Boolean? CanBeRecruitedAsDepartmentAdmin { get; set; }
			public AccessibilityType? Accessibility { get; set; }
		}
		Task<IResponse> SetCitizenSettings(IAsCitizenRequest<SetCitizenSettingsParameter> request);

		Task<IResponse> UnlinkCitizen(IAsCitizenRequest request);

		sealed class SearchCitizensParameter : EncryptableBase<Guid>
		{
			public String Name { get; set; }
			public IEnumerable<Guid> ExcludeIds { get; set; }
			public IEnumerable<String> ExcludeNames { get; set; }
			public AccessibilityType? Accessibility { get; set; }
			public Boolean? CanBeRecruitedAsDepartmentAdmin { get; set; }
			public Boolean? CanBeRecruitedAsAccountAdmin { get; set; }

			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				ExcludeIds = await decryptor.Decrypt(ExcludeIds);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				ExcludeIds = await encryptor.Encrypt(ExcludeIds);
			}
		}
		Task<IGetPaginatedEncryptableResponse<CitizenEntity>> SearchCitizens(IAsUserGetPaginatedEncryptableRequest<SearchCitizensParameter> request);

		Task<IEncryptableResponse<CitizenEntity>> RetrieveCitizen(String name);
	}
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
