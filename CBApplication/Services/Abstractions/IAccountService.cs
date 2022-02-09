using CBApplication.Requests.Abstractions;

using CBData.Abstractions;
using CBData.Entities;
using PBApplication.Responses.Abstractions;
using PBApplication.Services.Abstractions;
using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PBCommon.Enums;

namespace CBApplication.Services.Abstractions
{
	public interface IAccountService : IService
	{
		sealed class EditAccountAdminshipParameter : EncryptableBase<Guid>
		{
			public EditAccountAdminshipParameter()
			{
			}
			public EditAccountAdminshipParameter(Guid adminId)
			{
				AdminId = adminId;
			}

			public Guid AdminId { get; set; }

			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				AdminId = await decryptor.Decrypt(AdminId);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				AdminId = await encryptor.Encrypt(AdminId);
			}
		}
		Task<IResponse> ResignAdminFromAccount(IAsAccountEncryptableRequest<EditAccountAdminshipParameter> request);
		Task<IResponse> RecruitAdminIntoAccount(IAsAccountEncryptableRequest<EditAccountAdminshipParameter> request);

		abstract class SetAccountSettingsParameterBase : EncryptableBase<Guid>
		{
			public Dictionary<Guid, Boolean> CanReceiveTransactionOffersFor { get; set; }
			public Dictionary<Guid, Boolean> CanBeMiddlemanFor { get; set; }
			public TimeSpan? TransactionOfferLifetime { get; set; }
			public TimeSpan? MinimumContractLifeSpan { get; set; }
			public Boolean? CanBeRecruitedIntoDepartments { get; set; }
			public Boolean? ForcePriorityTags { get; set; }
			public AccessibilityType? Accessibility { get; set; }

			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				await decryptor.DecryptKeys(CanReceiveTransactionOffersFor);
				await decryptor.DecryptKeys(CanBeMiddlemanFor);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				await encryptor.EncryptKeys(CanReceiveTransactionOffersFor);
				await encryptor.EncryptKeys(CanBeMiddlemanFor);
			}
		}
		sealed class SetRealAccountSettingsParameter : SetAccountSettingsParameterBase
		{
			public Dictionary<Guid, Boolean> CanBeDepositAccountFor { get; set; }

			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				await base.DecryptSelf(decryptor);
				await decryptor.DecryptKeys(CanBeDepositAccountFor);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				await base.EncryptSelf(encryptor);
				await encryptor.EncryptKeys(CanBeDepositAccountFor);
			}
		}
		Task<IResponse> SetRealAccountSettings(IAsAccountEncryptableRequest<SetRealAccountSettingsParameter> request);

		abstract class SetVirtualAccountSettingsParameterBase : SetAccountSettingsParameterBase
		{
			public TimeSpan? DepositForwardLifeSpan { get; set; }
			public Decimal? DefaultDepositAccountMapRelativeLimit { get; set; }
			public Decimal? DefaultDepositAccountMapAbsoluteLimit { get; set; }
		}
		sealed class SetVirtualAccountSettingsParameter : SetVirtualAccountSettingsParameterBase
		{ }
		Task<IResponse> SetVirtualAccountSettings(IAsAccountEncryptableRequest<SetVirtualAccountSettingsParameter> request);

		sealed class SetDepartmentAccountSettingsParameter : SetVirtualAccountSettingsParameterBase
		{ }
		Task<IResponse> SetDepartmentAccountSettings(IAsAccountEncryptableRequest<SetDepartmentAccountSettingsParameter> request);

		Task<IGetPaginatedEncryptableResponse<DepositAccountReferenceEntity>> GetDepositReferencesForReferencedAccount(IAsAccountRequest request);
		Task<IGetPaginatedEncryptableResponse<DepositAccountReferenceEntity>> GetDepositReferencesForReferencingAccount(IAsAccountRequest request);

		Task<IEncryptableResponse<RealAccountSettingsEntity>> GetRealAccountSettings(IAsAccountRequest request);
		Task<IEncryptableResponse<VirtualAccountSettingsEntity>> GetVirtualAccountSettings(IAsAccountRequest request);
		Task<IEncryptableResponse<DepartmentAccountSettingsEntity>> GetDepartmentAccountSettings(IAsAccountRequest request);

		Task<IGetPaginatedEncryptableResponse<IAccountEntity>> GetAccounts(IAsCitizenRequest request);

		sealed class EditDepositAccountReferenceParameter : EncryptableBase<Guid>
		{
			public Guid AccountReferenceId { get; set; }
			public Decimal? AbsoluteLimit { get; set; }
			public Decimal? RelativeLimit { get; set; }
			public Boolean? IsActive { get; set; }
			public Boolean? UseAsForwarding { get; set; }

			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				AccountReferenceId = await decryptor.Decrypt(AccountReferenceId);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				AccountReferenceId = await encryptor.Encrypt(AccountReferenceId);
			}
		}
		Task<IResponse> EditDepositAccountReference(IAsAccountEncryptableRequest<EditDepositAccountReferenceParameter> request);

		sealed class DeleteDepositAccountReferenceParameter : EncryptableBase<Guid>
		{
			public Guid AccountReferenceId { get; set; }
			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				AccountReferenceId = await decryptor.Decrypt(AccountReferenceId);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				AccountReferenceId = await encryptor.Encrypt(AccountReferenceId);
			}
		}
		Task<IResponse> DeleteDepositAccountReference(IAsAccountEncryptableRequest<DeleteDepositAccountReferenceParameter> request);

		class CreateVirtualAccountParameter
		{
			public String Name { get; set; }
			public AccessibilityType Accessibility { get; set; }
		}
		Task<IResponse> CreateVirtualAccount(IAsCitizenRequest<CreateVirtualAccountParameter> request);

		sealed class CreateDepartmentAccountParameter : CreateVirtualAccountParameter, IEncryptable<Guid>
		{
			public Guid DepartmentId { get; set; }
			public Boolean IsEncrypted { get; private set; }

			public async Task Decrypt(IDecryptor<Guid> decryptor)
			{
				IsEncrypted = false;
				DepartmentId = await decryptor.Decrypt(DepartmentId);
			}

			public async Task Encrypt(IEncryptor<Guid> encryptor)
			{
				IsEncrypted = true;
				DepartmentId = await encryptor.Encrypt(DepartmentId);
			}
		}
		Task<IResponse> CreateDepartmentAccount(IAsCitizenEncryptableRequest<CreateDepartmentAccountParameter> request);

		sealed class CreateAccountReferenceParameter : EncryptableBase<Guid>
		{
			public Guid CurrencyId { get; set; }
			public Guid ReferencedAccountId { get; set; }

			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				CurrencyId = await decryptor.Decrypt(CurrencyId);
				ReferencedAccountId = await decryptor.Decrypt(ReferencedAccountId);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				CurrencyId = await encryptor.Encrypt(CurrencyId);
				ReferencedAccountId = await encryptor.Encrypt(ReferencedAccountId);
			}
		}
		Task<IResponse> CreateDepositAccountReference(IAsAccountEncryptableRequest<CreateAccountReferenceParameter> request);

		class SearchAccountsParameterBase : EncryptableBase<Guid>
		{
			public String Name { get; set; }
			public ICollection<String> ExcludeNames { get; set; }
			public AccessibilityType? Accessibility { get; set; }
			public Boolean? CanBeRecruitedIntoDepartments { get; set; }
			public Boolean? ForcePriorityTags { get; set; }
			public TimeSpan? TransactionOfferLifetime { get; set; }
			public TimeSpan? MinimumContractLifeSpan { get; set; }
			public Dictionary<Guid, Boolean> CanReceiveTransactionOffersFor { get; set; }
			public Dictionary<Guid, Boolean> CanCreateTransactionOffersFor { get; set; }
			public Dictionary<Guid, Boolean> CanBeMiddlemanFor { get; set; }
			public Guid? CreatorId { get; set; }
			public IEnumerable<Guid> ExcludeIds { get; set; }
			public ICollection<Guid> TagsIds { get; set; }
			public ICollection<Guid> PriorityTagsIds { get; set; }
			public ICollection<Guid> MangerIds { get; set; }

			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				await Task.WhenAll(
					decryptor.DecryptKeys(CanReceiveTransactionOffersFor),
					decryptor.DecryptKeys(CanCreateTransactionOffersFor),
					decryptor.DecryptKeys(CanBeMiddlemanFor));
				ExcludeIds = await decryptor.Decrypt(ExcludeIds);
				TagsIds = await decryptor.Decrypt(TagsIds);
				PriorityTagsIds = await decryptor.Decrypt(PriorityTagsIds);
				MangerIds = await decryptor.Decrypt(MangerIds);
				CreatorId = await decryptor.Decrypt(CreatorId);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				await Task.WhenAll(
					encryptor.EncryptKeys(CanReceiveTransactionOffersFor),
					encryptor.EncryptKeys(CanCreateTransactionOffersFor),
					encryptor.EncryptKeys(CanBeMiddlemanFor));
				ExcludeIds = await encryptor.Encrypt(ExcludeIds);
				TagsIds = await encryptor.Encrypt(TagsIds);
				PriorityTagsIds = await encryptor.Encrypt(PriorityTagsIds);
				MangerIds = await encryptor.Encrypt(MangerIds);
				CreatorId = await encryptor.Encrypt(CreatorId);
			}
		}
		Task<IGetPaginatedEncryptableResponse<IAccountEntity>> SearchAccounts(IAsAccountGetPaginatedEncryptableRequest<SearchAccountsParameterBase> request);

		sealed class SearchRealAccountsParameter : SearchAccountsParameterBase
		{
			public Dictionary<Guid, Boolean> CanBeDepositAccountFor { get; set; }

			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				await decryptor.DecryptKeys(CanBeDepositAccountFor);
				await base.DecryptSelf(decryptor);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				await encryptor.EncryptKeys(CanBeDepositAccountFor);
				await base.EncryptSelf(encryptor);
			}
		}
		Task<IGetPaginatedEncryptableResponse<RealAccountEntity>> SearchRealAccounts(IAsAccountGetPaginatedEncryptableRequest<SearchRealAccountsParameter> request);

		abstract class SearchVirtualAccountsParameterBase : SearchAccountsParameterBase
		{
			public TimeSpan? DepositForwardLifeSpan { get; set; }
		}
		Task<IGetPaginatedEncryptableResponse<IVirtualAccountEntity>> SearchVirtualAccounts(IAsAccountGetPaginatedEncryptableRequest<SearchVirtualAccountsParameterBase> request);

		sealed class SearchVirtualAccountsParameter : SearchVirtualAccountsParameterBase { }
		Task<IGetPaginatedEncryptableResponse<VirtualAccountEntity>> SearchVirtualAccounts(IAsAccountGetPaginatedEncryptableRequest<SearchVirtualAccountsParameter> request);

		sealed class SearchDepartmentAccountsParameter : SearchVirtualAccountsParameterBase { }
		Task<IGetPaginatedEncryptableResponse<DepartmentAccountEntity>> SearchDepartmentAccounts(IAsAccountGetPaginatedEncryptableRequest<SearchDepartmentAccountsParameter> request);
	}
}
