namespace CBApplication.Requests.Abstractions
{
    public interface IAsAccountGetPaginatedRequest : IAsCitizenGetPaginatedRequest, IAsAccountRequest { }
	public interface IAsAccountGetPaginatedRequest<TParameter> : IAsCitizenGetPaginatedRequest<TParameter>, IAsAccountRequest { }
	public interface IAsAccountGetPaginatedEncryptableRequest<TParameter> : IAsCitizenGetPaginatedEncryptableRequest<TParameter>, IAsAccountRequest where TParameter : IEncryptable<Guid> { }
}
