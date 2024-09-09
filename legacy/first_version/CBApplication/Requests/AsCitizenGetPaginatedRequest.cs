using CBApplication.Requests.Abstractions;

namespace CBApplication.Requests
{
    public class AsCitizenGetPaginatedRequest : AsUserGetPaginatedRequest, IAsCitizenGetPaginatedRequest
	{
		public Guid AsCitizenId { get; set; }
	}
	public class AsCitizenGetPaginatedRequest<TParameter> : AsUserGetPaginatedRequest<TParameter>, IAsCitizenGetPaginatedRequest<TParameter>
	{
		public Guid AsCitizenId { get; set; }
	}
	public class AsCitizenGetPaginatedEncryptableRequest<TParameter> : AsUserGetPaginatedEncryptableRequest<TParameter>, IAsCitizenGetPaginatedEncryptableRequest<TParameter>
		where TParameter : IEncryptable<Guid>
	{
		public Guid AsCitizenId { get; set; }
		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			await base.DecryptSelf(decryptor);
			AsCitizenId = await decryptor.Decrypt(AsCitizenId);
		}
		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			await base.EncryptSelf(encryptor);
			AsCitizenId = await encryptor.Encrypt(AsCitizenId);
		}
	}
}
