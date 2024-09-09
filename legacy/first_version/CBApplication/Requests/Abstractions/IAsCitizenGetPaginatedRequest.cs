namespace CBApplication.Requests.Abstractions
{
    public interface IAsCitizenGetPaginatedRequest : IAsUserGetPaginatedRequest, IAsCitizenRequest { }
	public interface IAsCitizenGetPaginatedRequest<TParameter> : IAsUserGetPaginatedRequest<TParameter>, IAsCitizenRequest { }
	public interface IAsCitizenGetPaginatedEncryptableRequest<TParameter> : IAsUserGetPaginatedEncryptableRequest<TParameter>, IAsCitizenRequest where TParameter : IEncryptable<Guid> { }
}
