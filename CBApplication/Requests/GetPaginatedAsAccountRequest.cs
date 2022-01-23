using CBApplication.Requests.Abstractions;
using PBCommon.Encryption.Abstractions;
using System;
using System.Threading.Tasks;

namespace CBApplication.Requests
{
	public class GetPaginatedAsAccountRequest : GetPaginatedAsCitizenRequest, IGetPaginatedAsAccountRequest
	{
		public Guid AsAccountId { get; set; }
	}
	public class GetPaginatedAsAccountRequest<TParameter> : GetPaginatedAsCitizenRequest<TParameter>, IGetPaginatedAsAccountRequest<TParameter>
	{
		public Guid AsAccountId { get; set; }
	}
	public class GetPaginatedAsAccountEncryptableRequest<TParameter> : GetPaginatedAsCitizenEncryptableRequest<TParameter>, IGetPaginatedAsAccountEncryptableRequest<TParameter>
		where TParameter : IEncryptable<Guid>
	{
		public Guid AsAccountId { get; set; }
		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			AsAccountId = await decryptor.Decrypt(AsAccountId);
			await base.DecryptSelf(decryptor);
		}
		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			AsAccountId = await encryptor.Encrypt(AsAccountId);
			await base.EncryptSelf(encryptor);
		}
	}
}
