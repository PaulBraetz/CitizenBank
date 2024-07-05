using CBApplication.Requests.Abstractions;

using PBApplication.Requests;
using PBCommon.Encryption.Abstractions;
using System;
using System.Threading.Tasks;

namespace CBApplication.Requests
{
	public class AsCitizenRequest : AsUserRequest, IAsCitizenRequest
	{
		public Guid AsCitizenId { get; set; }
	}
	public class AsCitizenRequest<TParameter> : AsUserRequest<TParameter>, IAsCitizenRequest<TParameter>
	{
		public Guid AsCitizenId { get; set; }
	}
	public class AsCitizenEncryptableRequest<TParameter> : AsUserEncryptableRequest<TParameter>, IAsCitizenEncryptableRequest<TParameter>
		where TParameter : IEncryptable<Guid>
	{
		public Guid AsCitizenId { get; set; }

		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			AsCitizenId = await decryptor.Decrypt(AsCitizenId);
			await base.DecryptSelf(decryptor);
		}
		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			AsCitizenId = await encryptor.Encrypt(AsCitizenId);
			await base.EncryptSelf(encryptor);
		}
	}
}
