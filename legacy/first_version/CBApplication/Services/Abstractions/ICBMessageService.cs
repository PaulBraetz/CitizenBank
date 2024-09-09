using CBApplication.Requests.Abstractions;

using CBData.Entities;

namespace CBApplication.Services.Abstractions
{
    public interface ICBMessageService : IService
	{
		sealed class GetCitizenMessagesParameter
		{
		}
		Task<IGetPaginatedEncryptableResponse<CitizenMessageEntity>> GetCitizenMessages(IAsCitizenGetPaginatedRequest<GetCitizenMessagesParameter> request);

		sealed class GetAccountMessagesParameter
		{
		}
		Task<IGetPaginatedEncryptableResponse<AccountMessageEntity>> GetAccountMessages(IAsAccountGetPaginatedRequest<GetAccountMessagesParameter> request);
	}
}
