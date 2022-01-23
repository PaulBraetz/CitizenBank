using CBApplication.Requests;
using CBApplication.Requests.Abstractions;

using CBData.Abstractions;
using CBData.Entities;
using PBApplication.Context.Abstractions;
using PBApplication.Extensions;
using PBApplication.Services;
using PBApplication.Services.Abstractions;
using PBCommon.Validation.Abstractions;
using PBData.Entities;
using PBData.Extensions;

using System;

namespace CBApplication.Services
{
	public abstract class CBService : DBConnectedService
	{
		protected CBService(IServiceContext serviceContext) : base(serviceContext)
		{
		}

		protected Lazy<CitizenEntity> GetCitizenEntityLazily(IAsCitizenRequest request)
		{
			return Connection.GetSingleLazily<CitizenEntity>(request.AsCitizenId);
		}
		protected Lazy<TAccount> GetAccountEntityLazily<TAccount>(IAsAccountRequest request)
			where TAccount : IAccountEntity
		{
			return Connection.GetSingleLazily<TAccount>(request.AsAccountId);
		}
		protected Lazy<IAccountEntity> GetAccountEntityLazily(IAsAccountRequest request)
		{
			return GetAccountEntityLazily<IAccountEntity>(request);
		}

		protected IValidationCriterionChain FirstValidateAsCitizen(UserEntity user, Lazy<CitizenEntity> citizen, IValidationFieldCollection fields)
		{
			return FirstValidateAsUser(user, fields)
				.NextManagerManagesProperty(user, citizen.Value, Connection, fields.GetField(nameof(AsCitizenRequest.AsCitizenId)));
		}
		protected IValidationCriterionChain FirstValidateAsAccount<TAccount>(UserEntity user, Lazy<CitizenEntity> citizen, Lazy<TAccount> account, IValidationFieldCollection fields)
			where TAccount : IAccountEntity
		{
			return FirstValidateAsCitizen(user, citizen, fields)
				.NextManagerManagesProperty(citizen, account, Connection, fields.GetField(nameof(AsAccountRequest.AsAccountId)));
		}
	}
}
