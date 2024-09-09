using CBApplication.Requests.Abstractions;

using CBData.Abstractions;
using CBData.Entities;

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

		protected TAccount GetAccountEntity<TAccount>(IAsAccountRequest request)
			where TAccount : IAccountEntity
		{
			return Connection.GetSingle<TAccount>(request.AsAccountId);
		}

		protected IAccountEntity GetAccountEntity(IAsAccountRequest request)
		{
			return GetAccountEntity<IAccountEntity>(request);
		}

		protected ISettingsEntity GetSettings(IEntity owner)
		{
			return GetSettings<ISettingsEntity>(owner);
		}
		protected TSettings GetSettings<TSettings>(IEntity owner)
			where TSettings : ISettingsEntity
		{
			return owner != null ? owner.GetHeldOwnerClaimsValues<TSettings>(Connection).SingleOrDefault() : default;
		}

		protected IValidationCriterionChain FirstValidateAsCitizen(IAsCitizenRequest request, IResponse response)
		{
			var user = GetUserEntity(request);
			var citizen = GetCitizenEntity(request);
			Boolean ownerCheck()
			{
				return user.HoldsOwnerRight(Connection, citizen);
			}

			return FirstValidateAuthenticatedDelegate(request, response)
				.NextNullCheck(citizen,
					ValidationField.Create(nameof(request.AsCitizenId)),
					ValidationCode.NotFound.WithMessage("The citizen provided could not be found."))
				.NextCompound(ownerCheck,
					ValidationCode.Unauthorized.WithMessage("You are not authorized to access this citizen."))
				.InheritField();
		}
		protected IValidationCriterionChain FirstValidateAsAccount(IAsAccountRequest request, IResponse response)
		{
			var citizen = GetCitizenEntity(request);
			var account = GetAccountEntity(request);
			Boolean adminOrOwnerCheck()
			{
				return citizen.HoldsOwnerRight(Connection, account) || citizen.HoldsAdminRight(Connection, account);
			}

			return FirstValidateAsCitizen(request, response)
				.NextNullCheck(account,
					ValidationField.Create(nameof(request.AsAccountId)),
					ValidationCode.NotFound.WithMessage("The account provided could not be found."))
				.NextCompound(adminOrOwnerCheck,
					ValidationCode.Unauthorized.WithMessage("You are not authorized to access this account."))
				.InheritField();
		}
	}
}
