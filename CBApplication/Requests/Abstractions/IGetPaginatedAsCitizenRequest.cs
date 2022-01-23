using PBApplication.Requests.Abstractions;
using PBCommon.Encryption.Abstractions;
using System;

namespace CBApplication.Requests.Abstractions
{
	public interface IGetPaginatedAsCitizenRequest : IGetPaginatedAsUserRequest, IAsCitizenRequest { }
	public interface IGetPaginatedAsCitizenRequest<TParameter> : IGetPaginatedAsUserRequest<TParameter>, IAsCitizenRequest { }
	public interface IGetPaginatedAsCitizenEncryptableRequest<TParameter> : IGetPaginatedAsUserEncryptableRequest<TParameter>, IAsCitizenRequest where TParameter : IEncryptable<Guid> { }
}
