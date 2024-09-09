using CBApplication.Requests.Abstractions;

using CBData.Entities;

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

		sealed class SetCurrentCitizenRequestParameter : EncryptableBase<Guid>
		{
			public Guid CitizenId { get; set; }

			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				CitizenId = await decryptor.Decrypt(CitizenId);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				CitizenId = await encryptor.Encrypt(CitizenId);
			}
		}
		Task<IResponse> SetCurrentCitizen(IEncryptableRequest<SetCurrentCitizenRequestParameter> request);
		Task<IEncryptableResponse<CitizenEntity>> GetCurrentCitizen();
	}
}
