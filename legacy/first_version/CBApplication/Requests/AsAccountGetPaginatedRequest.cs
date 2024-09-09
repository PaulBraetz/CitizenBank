using CBApplication.Requests.Abstractions;

namespace CBApplication.Requests
{
    public class AsAccountGetPaginatedRequest : AsCitizenGetPaginatedRequest, IAsAccountGetPaginatedRequest
	{
		public Guid AsAccountId { get; set; }
	}
	public class AsAccountGetPaginatedRequest<TParameter> : AsCitizenGetPaginatedRequest<TParameter>, IAsAccountGetPaginatedRequest<TParameter>
	{
		public Guid AsAccountId { get; set; }
	}
	public class AsAccountGetPaginatedEncryptableRequest<TParameter> : AsCitizenGetPaginatedEncryptableRequest<TParameter>, IAsAccountGetPaginatedEncryptableRequest<TParameter>
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
