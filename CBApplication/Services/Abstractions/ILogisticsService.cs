
using CBApplication.Requests.Abstractions;
using CBData.Entities;
using PBApplication.Requests.Abstractions;
using PBApplication.Responses.Abstractions;
using PBApplication.Services.Abstractions;
using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;
using System;
using System.Threading.Tasks;
using static CBCommon.Enums.LogisticsEnums;

namespace CBApplication.Services.Abstractions
{
	public interface ILogisticsService : IService
	{
		sealed class CreateLogisticsOrderParameter
		{
			public DateTimeOffset Deadline { get; set; }
			public String Origin { get; set; }
			public String Target { get; set; }
			public OrderType Type { get; set; }
			public String Details { get; set; }
		}
		Task<IResponse> CreateLogisticsOrder(IAsCitizenRequest<CreateLogisticsOrderParameter> request);

		sealed class EditLogisticsOrderParameter : EncryptableBase<Guid>
		{
			public Guid LogisticsOrderId { get; set; }
			public OrderStatus Status { get; set; }

			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				LogisticsOrderId = await decryptor.Decrypt(LogisticsOrderId);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				LogisticsOrderId = await encryptor.Encrypt(LogisticsOrderId);
			}
		}
		Task<IResponse> EditLogisticsOrder(IAsCitizenEncryptableRequest<EditLogisticsOrderParameter> request);
		sealed class GetLogisticsOrdersParameter : EncryptableBase<Guid>
		{
			public OrderStatus Status { get; set; }
			public DateTimeOffset Deadline { get; set; }
			public String Origin { get; set; }
			public String Target { get; set; }
			public OrderType Type { get; set; }
			public String Details { get; set; }
			public Guid ClientId { get; set; }

			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				ClientId = await decryptor.Decrypt(ClientId);
			}
			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				ClientId = await encryptor.Encrypt(ClientId);
			}
		}
		Task<IGetPaginatedEncryptableResponse<LogisticsOrderEntity>> GetLogisticsOrders(IGetPaginatedEncryptableRequest<GetLogisticsOrdersParameter> request);
	}
}
