using CBApplication.Requests;
using CBApplication.Requests.Abstractions;

using CBData.Abstractions;
using CBData.Entities;
using PBApplication.Context.Abstractions;
using PBApplication.Extensions;
using PBApplication.Responses.Abstractions;
using PBApplication.Services;
using PBApplication.Services.Abstractions;
using PBCommon.Validation;
using PBCommon.Validation.Abstractions;
using PBData.Abstractions;
using PBData.Entities;
using PBData.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace CBApplication.Services
{
	public abstract class CBService : DBConnectedService
	{
		protected CBService(IServiceContext serviceContext) : base(serviceContext)
		{
		}

		protected CitizenEntity GetCitizenEntity(IAsCitizenRequest request)
		{
			return Connection.GetSingle<CitizenEntity>(request.AsCitizenId);
		}
		protected Lazy<CitizenEntity> GetCitizenEntityLazily(IAsCitizenRequest request)
		{
			return Connection.GetSingleLazily<CitizenEntity>(request.AsCitizenId);
		}

		protected TAccount GetAccountEntity<TAccount>(IAsAccountRequest request)
			where TAccount : IAccountEntity
		{
			return Connection.GetSingle<TAccount>(request.AsAccountId);
		}
		protected Lazy<TAccount> GetAccountEntityLazily<TAccount>(IAsAccountRequest request)
			where TAccount : IAccountEntity
		{
			return Connection.GetSingleLazily<TAccount>(request.AsAccountId);
		}

		protected IAccountEntity GetAccountEntity(IAsAccountRequest request)
		{
			return GetAccountEntity<IAccountEntity>(request);
		}
		protected Lazy<IAccountEntity> GetAccountEntityLazily(IAsAccountRequest request)
		{
			return GetAccountEntityLazily<IAccountEntity>(request);
		}

		protected ISettingsEntity GetSettings(IEntity owner)
		{
			return GetSettings<ISettingsEntity>(owner);
		}
		protected TSettings GetSettings<TSettings>(IEntity owner)
			where TSettings : ISettingsEntity
		{
			return owner.GetHeldOwnerClaimsValues<TSettings>(Connection).SingleOrDefault();
		}

		protected IValidationCriterionChain FirstValidateAsCitizen(IAsCitizenRequest request, IResponse response)
		{
			var user = GetUserEntityLazily(request);
			var citizen = GetCitizenEntityLazily(request);
			Boolean ownerCheck()
			{
				return user.Value.HoldsOwnerRight(Connection, citizen.Value);
			}

			return FirstValidateAuthenticatedDelegate(request, response)
				.NextNullCheck(citizen,
					response.Validation.GetField(nameof(request.AsCitizenId)),
					DefaultCode.NotFound.SetMessage("The citizen provided could not be found."))
				.NextCompound(ownerCheck,
					response.Validation.GetField(nameof(request.AsCitizenId)),
					DefaultCode.Unauthorized.SetMessage("You are not authorized to access this citizen."));
		}
		protected IValidationCriterionChain FirstValidateAsAccount(IAsAccountRequest request, IResponse response)
		{
			var citizen = GetCitizenEntityLazily(request);
			var account = GetAccountEntityLazily(request);
			Boolean adminOrOwnerCheck()
			{
				return citizen.Value.HoldsOwnerRight(Connection, account.Value) || citizen.Value.HoldsAdminRight(Connection, account.Value);
			}

			return FirstValidateAsCitizen(request, response)
				.NextNullCheck(account,
					response.Validation.GetField(nameof(request.AsAccountId)),
					DefaultCode.NotFound.SetMessage("The account provided could not be found."))
				.NextCompound(adminOrOwnerCheck,
					response.Validation.GetField(nameof(request.AsAccountId)),
					DefaultCode.Unauthorized.SetMessage("You are not authorized to access this account."));
		}
	}
}
