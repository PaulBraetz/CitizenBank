using CBApplication.Requests.Abstractions;
using PBCommon.Encryption.Abstractions;
using System;
using System.Threading.Tasks;

namespace CBApplication.Requests
{
	public class AsAccountRequest : AsCitizenRequest, IAsAccountRequest
	{
		public Guid AsAccountId { get; set; }
	}
	public class AsAccountRequest<TParameter> : AsCitizenRequest<TParameter>, IAsAccountRequest<TParameter>
	{
		public Guid AsAccountId { get; set; }
	}
	public class AsAccountEncryptableRequest<TParameter> : AsCitizenEncryptableRequest<TParameter>, IAsAccountEncryptableRequest<TParameter>
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
