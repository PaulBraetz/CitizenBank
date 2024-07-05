using PBCommon.Encryption.Abstractions;
using System;

namespace CBApplication.Requests.Abstractions
{
	public interface IAsAccountRequest : IAsCitizenRequest
	{
		public Guid AsAccountId { get; set; }
	}
	public interface IAsAccountRequest<TParameter> : IAsCitizenRequest<TParameter>, IAsAccountRequest { }
	public interface IAsAccountEncryptableRequest<TParameter> : IAsCitizenEncryptableRequest<TParameter>, IAsAccountRequest where TParameter : IEncryptable<Guid> { }
}
