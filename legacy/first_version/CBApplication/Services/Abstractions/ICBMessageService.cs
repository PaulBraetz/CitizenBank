using CBApplication.Requests.Abstractions;

using CBData.Entities;
using PBApplication.Responses.Abstractions;
using PBApplication.Services.Abstractions;
using System.Threading.Tasks;

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
