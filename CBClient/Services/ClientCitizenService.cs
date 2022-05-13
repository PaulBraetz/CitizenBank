using CBApplication.Requests.Abstractions;
using CBApplication.Services.Abstractions;
using CBData.Entities;
using PBApplication.Context.Abstractions;
using PBApplication.Requests.Abstractions;
using PBApplication.Responses;
using PBApplication.Responses.Abstractions;
using PBClient.Access;
using PBClient.Services;
using System;
using System.Threading.Tasks;

namespace CitizenBank.Client.Services
{
	internal sealed class ClientCitizenService : ClientService, ICitizenService
	{
		public ClientCitizenService(IServiceContext serviceContext, WebClient webClient) : base(serviceContext, webClient)
		{
		}

		public async Task<IResponse> CancelCitizenLinkRequest(IAsUserEncryptableRequest<ICitizenService.CancelCitizenLinkRequestParameter> request)
		{
			return await WebClient.SerializePostDeserialize<IAsUserEncryptableRequest<ICitizenService.CancelCitizenLinkRequestParameter>, Response>("api/citizen/CancelCitizenLinkRequest", request);
		}

		public async Task<IResponse> CreateCitizenLinkRequest(IAsUserRequest<ICitizenService.CreateCitizenLinkRequestParameter> request)
		{
			return await WebClient.SerializePostDeserialize<IAsUserRequest<ICitizenService.CreateCitizenLinkRequestParameter>, Response>("api/citizen/CreateCitizenLinkRequest", request);
		}

		public async Task<IGetPaginatedEncryptableResponse<CitizenLinkRequestEntity>> GetCitizenLinkRequests(IAsUserGetPaginatedRequest<ICitizenService.GetCitizenLinkRequestsParameter> request)
		{
			return await WebClient.SerializePostDeserialize<IAsUserGetPaginatedRequest<ICitizenService.GetCitizenLinkRequestsParameter>, IGetPaginatedEncryptableResponse<CitizenLinkRequestEntity>>("api/citizen/GetCitizenLinkRequests", request);
		}

		public async Task<IGetPaginatedEncryptableResponse<CitizenLinkRequestEntity>> GetCitizenLinkRequests()
		{
			return await WebClient.GetDeserialize<GetPaginatedEncryptableResponse<CitizenLinkRequestEntity>>("api/citizen/GetCitizenLinkRequests");
		}

		public async Task<IGetPaginatedEncryptableResponse<CitizenEntity>> GetCitizens(IAsUserGetPaginatedRequest<ICitizenService.GetCitizensParameter> request)
		{
			return await WebClient.SerializePostDeserialize<IAsUserGetPaginatedRequest<ICitizenService.GetCitizensParameter>, GetPaginatedEncryptableResponse<CitizenEntity>>("api/citizen/GetCitizens", request);
		}

		public async Task<IGetPaginatedEncryptableResponse<CitizenEntity>> GetCitizens()
		{
			return await WebClient.GetDeserialize<GetPaginatedEncryptableResponse<CitizenEntity>>("api/citizen/GetCitizens");
		}

		public async Task<IEncryptableResponse<CitizenSettingsEntity>> GetCitizenSettings(IAsCitizenRequest request)
		{
			return await WebClient.SerializePostDeserialize<IAsCitizenRequest, EncryptableResponse<CitizenSettingsEntity>>("api/citizen/GetCitizenSettings", request);
		}

		public async Task<IEncryptableResponse<CitizenEntity>> GetCurrentCitizen()
		{
			return await WebClient.GetDeserialize<EncryptableResponse<CitizenEntity>>("api/citizen/GetCurrentCitizen");
		}

		public async Task<IEncryptableResponse<CitizenEntity>> RetrieveCitizen(String name)
		{
			return await WebClient.PostDeserialize<EncryptableResponse<CitizenEntity>>("api/citizen/RetrieveCitizen", name);
		}

		public async Task<IGetPaginatedEncryptableResponse<CitizenEntity>> SearchCitizens(IAsUserGetPaginatedEncryptableRequest<ICitizenService.SearchCitizensParameter> request)
		{
			return await WebClient.SerializePostDeserialize<IAsUserGetPaginatedEncryptableRequest<ICitizenService.SearchCitizensParameter>, GetPaginatedEncryptableResponse<CitizenEntity>>("api/citizen/SearchCitizens", request);
		}

		public async Task<IResponse> SetCitizenSettings(IAsCitizenRequest<ICitizenService.SetCitizenSettingsParameter> request)
		{
			return await WebClient.SerializePostDeserialize<IAsCitizenRequest<ICitizenService.SetCitizenSettingsParameter>, Response>("api/citizen/SetCitizenSettings", request);
		}

		public async Task<IResponse> SetCurrentCitizen(IEncryptableRequest<ICitizenService.SetCurrentCitizenRequestParameter> request)
		{
			return await WebClient.SerializePostDeserialize<IEncryptableRequest<ICitizenService.SetCurrentCitizenRequestParameter>, Response>("api/citizen/SetCurrentCitizen", request);
		}

		public async Task<IResponse> UnlinkCitizen(IAsCitizenRequest request)
		{
			return await WebClient.SerializePostDeserialize<IAsCitizenRequest, Response>("api/citizen/UnlinkCitizen", request);
		}

		public async Task<IResponse> VerifyCitizenLinkRequest(IAsUserEncryptableRequest<ICitizenService.VerifyCitizenLinkRequestParameter> request)
		{
			return await WebClient.SerializePostDeserialize<IAsUserEncryptableRequest<ICitizenService.VerifyCitizenLinkRequestParameter>, Response>("api/citizen/VerifyCitizenLinkRequest", request);
		}
	}
}
