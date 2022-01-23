using PBApplication.Requests.Abstractions;
using PBCommon.Encryption.Abstractions;
using System;

namespace CBApplication.Requests.Abstractions
{
	public interface IGetPaginatedAsAccountRequest : IGetPaginatedAsCitizenRequest, IAsAccountRequest	{	}
	public interface IGetPaginatedAsAccountRequest<TParameter> : IGetPaginatedAsCitizenRequest<TParameter>, IAsAccountRequest	{ }
	public interface IGetPaginatedAsAccountEncryptableRequest<TParameter> : IGetPaginatedAsCitizenEncryptableRequest<TParameter>, IAsAccountRequest where TParameter : IEncryptable<Guid> { }
}
