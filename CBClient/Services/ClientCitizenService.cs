using CBApplication.Requests.Abstractions;
using CBApplication.Services.Abstractions;
using CBData.Entities;
using PBApplication.Context.Abstractions;
using PBApplication.Requests.Abstractions;
using PBApplication.Responses;
using PBApplication.Responses.Abstractions;
using PBApplication.Services.Abstractions;
using PBClient.Access;
using PBClient.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CitizenBank.Client.Services
{
	internal sealed class ClientCitizenService : ClientService, ICitizenServiceBase
	{
		public ClientCitizenService(IServiceContext serviceContext, WebClient webClient) : base(serviceContext, webClient)
		{
		}

		public async Task<IResponse> CancelCitizenLinkRequest(IAsUserEncryptableRequest<ICitizenServiceBase.CancelCitizenLinkRequestParameter> request)
		{
			return await WebClient.SerializePostDeserialize<IAsUserEncryptableRequest<ICitizenServiceBase.CancelCitizenLinkRequestParameter>, Response>("api/citizen/CancelCitizenLinkRequest", request);
		}

		public async Task<IResponse> CreateCitizenLinkRequest(IAsUserRequest<ICitizenServiceBase.CreateCitizenLinkRequestParameter> request)
		{
			return await WebClient.SerializePostDeserialize<IAsUserRequest<ICitizenServiceBase.CreateCitizenLinkRequestParameter>, Response>("api/citizen/CreateCitizenLinkRequest", request);
		}

		public async Task<IGetPaginatedEncryptableResponse<CitizenLinkRequestEntity>> GetCitizenLinkRequests(IGetPaginatedAsUserRequest<ICitizenServiceBase.GetCitizenLinkRequestsParameter> request)
		{
			return await WebClient.SerializePostDeserialize<IGetPaginatedAsUserRequest<ICitizenServiceBase.GetCitizenLinkRequestsParameter>, IGetPaginatedEncryptableResponse<CitizenLinkRequestEntity>>("api/citizen/GetCitizenLinkRequests", request);
		}

		public async Task<IGetPaginatedEncryptableResponse<CitizenLinkRequestEntity>> GetCitizenLinkRequests()
		{
			return await WebClient.GetDeserialize<GetPaginatedEncryptableResponse<CitizenLinkRequestEntity>>("api/citizen/GetCitizenLinkRequests");
		}

		public async Task<IGetPaginatedEncryptableResponse<CitizenEntity>> GetCitizens(IGetPaginatedAsUserRequest<ICitizenServiceBase.GetCitizensParameter> request)
		{
			return await WebClient.SerializePostDeserialize<IGetPaginatedAsUserRequest<ICitizenServiceBase.GetCitizensParameter>, GetPaginatedEncryptableResponse<CitizenEntity>>("api/citizen/GetCitizens", request);
		}

		public async Task<IGetPaginatedEncryptableResponse<CitizenEntity>> GetCitizens()
		{
			return await WebClient.GetDeserialize<GetPaginatedEncryptableResponse<CitizenEntity>>("api/citizen/GetCitizens");
		}

		public async Task<IEncryptableResponse<CitizenSettingsEntity>> GetCitizenSettings(IAsCitizenRequest request)
		{
			return await WebClient.SerializePostDeserialize<IAsCitizenRequest, EncryptableResponse<CitizenSettingsEntity>>("api/citizen/GetCitizenSettings", request);
		}

		public async Task<IEncryptableResponse<CitizenEntity>> RetrieveCitizen(String name)
		{
			return await WebClient.PostDeserialize<IEncryptableResponse<CitizenEntity>>("api/citizen/RetrieveCitizen", name);
		}

		public async Task<IGetPaginatedEncryptableResponse<CitizenEntity>> SearchCitizens(IGetPaginatedAsUserEncryptableRequest<ICitizenServiceBase.SearchCitizensParameter> request)
		{
			return await WebClient.SerializePostDeserialize<IGetPaginatedAsUserEncryptableRequest<ICitizenServiceBase.SearchCitizensParameter>, GetPaginatedEncryptableResponse<CitizenEntity>>("api/citizen/SearchCitizens", request);
		}

		public async Task<IResponse> SetCitizenSettings(IAsCitizenRequest<ICitizenServiceBase.SetCitizenSettingsParameter> request)
		{
			return await WebClient.SerializePostDeserialize< IAsCitizenRequest < ICitizenServiceBase.SetCitizenSettingsParameter > ,Response >("api/citizen/SetCitizenSettings", request);
		}

		public async Task<IResponse> UnlinkCitizen(IAsCitizenRequest request)
		{
			return await WebClient.SerializePostDeserialize<IAsCitizenRequest, Response>("api/citizen/UnlinkCitizen", request);
		}

		public async Task<IResponse> VerifyCitizenLinkRequest(IAsUserEncryptableRequest<ICitizenServiceBase.VerifyCitizenLinkRequestParameter> request)
		{
			return await WebClient.SerializePostDeserialize<IAsUserEncryptableRequest<ICitizenServiceBase.VerifyCitizenLinkRequestParameter>, Response>("api/citizen/VerifyCitizenLinkRequest", request);
		}
	}
}
