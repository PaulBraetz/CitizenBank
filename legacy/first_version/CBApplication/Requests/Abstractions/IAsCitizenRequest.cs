namespace CBApplication.Requests.Abstractions
{
    public interface IAsCitizenRequest : IAsUserRequest
	{
		public Guid AsCitizenId { get; set; }
	}
	public interface IAsCitizenRequest<TParameter> : IAsUserRequest<TParameter>, IAsCitizenRequest { }
	public interface IAsCitizenEncryptableRequest<TParameter> : IAsUserEncryptableRequest<TParameter>, IAsCitizenRequest where TParameter : IEncryptable<Guid> { }
}
