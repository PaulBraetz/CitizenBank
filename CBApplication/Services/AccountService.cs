using CBApplication.Extensions;
using CBApplication.Requests;
using CBApplication.Requests.Abstractions;
using CBApplication.Services.Abstractions;

using CBCommon.Components;

using CBData.Abstractions;
using CBData.Entities;



using PBApplication.Events;
using PBApplication.Extensions;
using PBApplication.Responses;
using PBApplication.Responses.Abstractions;
using PBApplication.Services.Abstractions;
using PBCommon.Validation;
using PBCommon.Validation.Abstractions;

using PBCommon.Extensions;

using PBData.Abstractions;
using PBData.Entities;
using PBData.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;

using static CBApplication.Services.Abstractions.IEventfulAccountService;
using static CBCommon.Enums.CitizenBankEnums;
using static PBCommon.Enums;
using System.Threading.Tasks;
using static CBApplication.Services.Abstractions.IAccountService;
using PBApplication.Context.Abstractions;

namespace CBApplication.Services
{
	public class AccountService : CBService, IEventfulAccountService
	{
		public AccountService(IServiceContext serviceContext) : base(serviceContext)
		{
			Observe<IEventfulAccountService>(this);
		}

		public event ServiceEventHandler<ServiceEventArgs<CitizenEntity>> OnAdminRecruitedForAccount;
		public event ServiceEventHandler<ServiceEventArgs<VirtualAccountEntity>> OnAdminRecruitedForAdmin;
		public event ServiceEventHandler<ServiceEventArgs<CitizenEntity>> OnAdminResignedForAccount;
		public event ServiceEventHandler<ServiceEventArgs<VirtualAccountEntity>> OnAdminResignedForAdmin;
		public event ServiceEventHandler<ServiceEventArgs<RealAccountSettingsEntity>> OnRealAccountSettingsChanged;
		public event ServiceEventHandler<ServiceEventArgs<VirtualAccountSettingsEntity>> OnVirtualAccountSettingsChanged;
		public event ServiceEventHandler<ServiceEventArgs<DepartmentAccountSettingsEntity>> OnDepartmentAccountSettingsChanged;
		public event ServiceEventHandler<ServiceEventArgs<DepositAccountReferenceEntity>> OnDepositAccountReferenceChangedForReferencing;
		public event ServiceEventHandler<ServiceEventArgs<DepositAccountReferenceEntity>> OnDepositAccountReferenceChangedForReferenced;
		public event ServiceEventHandler<ServiceEventArgs<VirtualAccountEntity>> OnVirtualAccountCreated;
		public event ServiceEventHandler<ServiceEventArgs<DepartmentAccountEntity>> OnDepartmentAccountCreated;
		public event ServiceEventHandler<ServiceEventArgs<DepositAccountReferenceEntity>> OnDepositAccountReferenceCreatedForReferenced;
		public event ServiceEventHandler<ServiceEventArgs<DepositAccountReferenceEntity>> OnDepositAccountReferenceCreatedForReferencing;
		public event ServiceEventHandler<ServiceEventArgs> OnDepositAccountReferenceDeleted;
		public event ServiceEventHandler<ServiceEventArgs<Decimal>> OnDepositBalanceUpdated;

		public async Task<IResponse> RecruitAdminIntoAccount(IAsAccountEncryptableRequest<EditAccountAdminshipParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<VirtualAccountEntity> account = GetAccountEntityLazily<VirtualAccountEntity>(request);
				Lazy<CitizenEntity> admin = Connection.GetSingleLazily<CitizenEntity>(request.Parameter.AdminId);
				Lazy<CitizenSettingsEntity> settings = Connection.GetSingleLazily<CitizenSettingsEntity>(s => s.Owner.Id == admin.Value.Id);

				Boolean canBeRecruitedAsAdminCheck()
				{
					return settings.Value.CanBeRecruitedAsAccountAdmin;
				}
				void successAction()
				{
					account.Value.Admins.Remove(admin.Value);
					Connection.Update(account);
					Connection.SaveChanges();

					OnAdminRecruitedForAccount.Invoke(Session, account.Value, admin.Value.CloneAsT());
					OnAdminRecruitedForAdmin.Invoke(Session, admin.Value, account.Value.CloneAsT());

					LogIfAccessingAsDelegate(user, "added " + admin.Value.Name + " to account " + account.Value.Name);
				}

				await FirstValidateAsCitizen(user, citizen, response.Validation)
					.NextOwnerOwnsProperty(citizen, account, Connection, response.Validation.GetField(nameof(request.AsAccountId)))
					.NextNullCheck(admin.Value,
						response.Validation.GetField(nameof(request.Parameter.AdminId)),
						DefaultCode.NotFound.SetMessage("The admin requested could not be found."))
					.NextNullCheck(account.Value.Admins.SingleOrDefault(a => a.Id == admin.Value.Id),
						DefaultCode.Duplicate.SetMessage("The admin requested has already been recruited."))
					.InvertCriterion()
					.NextCompound(canBeRecruitedAsAdminCheck,
						DefaultCode.Unauthorized.SetMessage("The admin requested can not be recruited."))
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IResponse> ResignAdminFromAccount(IAsAccountEncryptableRequest<EditAccountAdminshipParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<VirtualAccountEntity> account = GetAccountEntityLazily<VirtualAccountEntity>(request);
				Lazy<CitizenEntity> admin = new Lazy<CitizenEntity>(() => account.Value.Admins.SingleOrDefault(a => a.Id == request.Parameter.AdminId));

				void successAction()
				{
					account.Value.Admins.Remove(admin.Value);
					Connection.Update(account.Value);
					Connection.SaveChanges();

					OnAdminResignedForAccount.Invoke(Session, account.Value, admin.Value.CloneAsT());
					OnAdminResignedForAdmin.Invoke(Session, admin.Value, account.Value.CloneAsT());

					LogIfAccessingAsDelegate(user, "removed " + admin.Value.Name + " from account " + account.Value.Name);
				}

				await FirstValidateAsCitizen(user, citizen, response.Validation)
					.NextOwnerOwnsProperty(citizen, account, Connection, response.Validation.GetField(nameof(request.AsAccountId)))
					.NextNullCheck(admin.Value,
						response.Validation.GetField(nameof(request.Parameter.AdminId)),
						DefaultCode.NotFound.SetMessage("The admin requested could not be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		private async Task<Boolean> TrySetCurrencyBoolDictionaryEntity(Lazy<CurrencyBoolDictionaryEntity> dict, IDictionary<Guid, Boolean> requestDict, IValidationFieldCollection fields)
		{
			Boolean changed = false;
			if (requestDict.Any())
			{
				foreach (var kvp in requestDict)
				{
					Lazy<CurrencyEntity> currency = Connection.GetSingleLazily<CurrencyEntity>(kvp.Key);
					Boolean check()
					{
						return dict.Value[currency.Value] != kvp.Value;
					}
					void successAction()
					{
						dict.Value[currency.Value] = kvp.Value;
						changed = true;
					}
					await FirstNullCheck(currency, fields.GetField(nameof(requestDict)), DefaultCode.PartiallyNotFound)
						.NextCompound(check)
						.SetOnCriterionMet(successAction)
						.Evaluate();
				}
			}
			return changed;
		}

		private async Task<Boolean> TrySetAccountSettingsBase<TSettings, TParameter>(Lazy<TSettings> settings, TParameter parameter, IValidationFieldCollection fields)
			where TSettings : IAccountSettingsEntity
			where TParameter : SetAccountSettingsParameterBase
		{
			var changed = false;

			var changedCanBeMiddleManFor = await TrySetCurrencyBoolDictionaryEntity(new Lazy<CurrencyBoolDictionaryEntity>(() => settings.Value.CanBeMiddlemanFor), parameter.CanBeMiddlemanFor, fields);
			var changedCanReceiveTransactionOffersFor = await TrySetCurrencyBoolDictionaryEntity(new Lazy<CurrencyBoolDictionaryEntity>(() => settings.Value.CanReceiveTransactionOffersFor), parameter.CanReceiveTransactionOffersFor, fields);

			changed = changedCanBeMiddleManFor || changedCanReceiveTransactionOffersFor;

			var tasks = new List<Task>();

			Boolean transactionOfferLifetimeCheck()
			{
				return settings.Value.TransactionOfferLifetime != parameter.TransactionOfferLifetime.Value;
			}
			void transactionOfferLifetimeSuccessAction()
			{
				settings.Value.TransactionOfferLifetime = parameter.TransactionOfferLifetime.Value;
				changed = true;
			}

			tasks.Add(FirstNullCheck(parameter.TransactionOfferLifetime)
				.NextCompound(transactionOfferLifetimeCheck)
				.SetOnCriterionMet(transactionOfferLifetimeSuccessAction)
				.Evaluate());

			Boolean minimumContractLifeSpanCheck()
			{
				return settings.Value.MinimumContractLifeSpan != parameter.MinimumContractLifeSpan.Value;
			}
			void minimumContractLifeSpanSuccessAction()
			{
				settings.Value.MinimumContractLifeSpan = parameter.MinimumContractLifeSpan.Value;
				changed = true;
			}

			tasks.Add(FirstNullCheck(parameter.MinimumContractLifeSpan)
				.NextCompound(minimumContractLifeSpanCheck)
				.SetOnCriterionMet(minimumContractLifeSpanSuccessAction)
				.Evaluate()); ;

			Boolean canBeRecruitedIntoDepartmentsCheck()
			{
				return settings.Value.CanBeRecruitedIntoDepartments != parameter.CanBeRecruitedIntoDepartments.Value;
			}
			void canBeRecruitedIntoDepartmentsCheckSuccessAction()
			{
				settings.Value.CanBeRecruitedIntoDepartments = parameter.CanBeRecruitedIntoDepartments.Value;
				changed = true;
			}

			tasks.Add(FirstNullCheck(parameter.CanBeRecruitedIntoDepartments)
				.NextCompound(canBeRecruitedIntoDepartmentsCheck)
				.SetOnCriterionMet(canBeRecruitedIntoDepartmentsCheckSuccessAction)
				.Evaluate());

			Boolean forcePriorityTagsCheck()
			{
				return settings.Value.ForcePriorityTags != parameter.ForcePriorityTags.Value;
			}
			void forcePriorityTagsCheckSuccessAction()
			{
				settings.Value.ForcePriorityTags = parameter.ForcePriorityTags.Value;
				changed = true;
			}

			tasks.Add(FirstNullCheck(parameter.ForcePriorityTags)
				.NextCompound(forcePriorityTagsCheck)
				.SetOnCriterionMet(forcePriorityTagsCheckSuccessAction)
				.Evaluate());

			Boolean accessibilityCheck()
			{
				return settings.Value.Accessibility != parameter.Accessibility.Value;
			}
			void accessibilitySuccessAction()
			{
				settings.Value.Accessibility = parameter.Accessibility.Value;
				changed = true;
			}

			tasks.Add(FirstNullCheck(parameter.Accessibility)
				.NextCompound(accessibilityCheck)
				.SetOnCriterionMet(accessibilitySuccessAction)
				.Evaluate());

			await Task.WhenAll(tasks);

			return changed;
		}

		public async Task<IResponse> SetRealAccountSettings(IAsAccountEncryptableRequest<SetRealAccountSettingsParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{

				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<RealAccountEntity> account = GetAccountEntityLazily<RealAccountEntity>(request);
				Lazy<RealAccountSettingsEntity> settings = Connection.GetSingleLazily<RealAccountSettingsEntity>(s => s.Id == account.Value.Id);

				async Task successAction()
				{
					Boolean changed = await TrySetAccountSettingsBase<RealAccountSettingsEntity, SetRealAccountSettingsParameter>(settings, request.Parameter, response.Validation);
					Boolean changedCanBeMiddleManFor = await TrySetCurrencyBoolDictionaryEntity(new Lazy<CurrencyBoolDictionaryEntity>(() => settings.Value.CanBeMiddlemanFor), request.Parameter.CanBeDepositAccountFor, response.Validation);

					changed = changed || changedCanBeMiddleManFor;

					if (changed)
					{
						Connection.Update(settings);
						Connection.SaveChanges();

						OnRealAccountSettingsChanged.Invoke(Session, settings.Value, settings.Value.CloneAsT());

						LogIfAccessingAsDelegate(user, $"set account settings for {account.Value.Name}");
					}
				}

				await FirstValidateAsAccount(user, citizen, account, response.Validation)
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		private async Task<Boolean> TrySetVirtualAccountSettingsBase<TSettings, TParameter>(Lazy<TSettings> settings, TParameter parameter, IValidationFieldCollection fields)
			where TParameter : SetVirtualAccountSettingsParameterBase
			where TSettings : IVirtualAccountSettingsEntity
		{
			Boolean changed = await TrySetAccountSettingsBase<TSettings, TParameter>(settings, parameter, fields);

			var tasks = new List<Task>();

			Boolean depositForwardLifeSpanCheck()
			{
				return settings.Value.DepositForwardLifeSpan != parameter.DepositForwardLifeSpan.Value;
			}
			void depositForwardLifeSpanSuccessAction()
			{
				settings.Value.DepositForwardLifeSpan = parameter.DepositForwardLifeSpan.Value;
				changed = true;
			}

			tasks.Add(FirstNullCheck(parameter.DepositForwardLifeSpan)
				.NextCompound(depositForwardLifeSpanCheck)
				.SetOnCriterionMet(depositForwardLifeSpanSuccessAction)
				.Evaluate());

			Boolean defaultDepositAccountMapRelativeLimitCheck()
			{
				return settings.Value.DefaultDepositAccountMapRelativeLimit != parameter.DefaultDepositAccountMapRelativeLimit.Value;
			}
			void defaultDepositAccountMapRelativeLimitSuccessAction()
			{
				settings.Value.DefaultDepositAccountMapRelativeLimit = parameter.DefaultDepositAccountMapRelativeLimit.Value;
				changed = true;
			}

			tasks.Add(FirstNullCheck(parameter.DefaultDepositAccountMapRelativeLimit)
				.NextCompound(defaultDepositAccountMapRelativeLimitCheck)
				.SetOnCriterionMet(defaultDepositAccountMapRelativeLimitSuccessAction)
				.Evaluate());

			Boolean defaultDepositAccountMapAbsoluteLimitCheck()
			{
				return settings.Value.DefaultDepositAccountMapAbsoluteLimit != parameter.DefaultDepositAccountMapAbsoluteLimit.Value;
			}
			void defaultDepositAccountMapAbsoluteLimitSuccessAction()
			{
				settings.Value.DefaultDepositAccountMapAbsoluteLimit = parameter.DefaultDepositAccountMapAbsoluteLimit.Value;
				changed = true;
			}

			tasks.Add(FirstNullCheck(parameter.DefaultDepositAccountMapAbsoluteLimit)
				.NextCompound(defaultDepositAccountMapAbsoluteLimitCheck)
				.SetOnCriterionMet(defaultDepositAccountMapAbsoluteLimitSuccessAction)
				.Evaluate());

			await Task.WhenAll(tasks);

			return changed;
		}

		public async Task<IResponse> SetVirtualAccountSettings(IAsAccountEncryptableRequest<SetVirtualAccountSettingsParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{

				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<VirtualAccountEntity> account = GetAccountEntityLazily<VirtualAccountEntity>(request);
				Lazy<VirtualAccountSettingsEntity> settings = Connection.GetSingleLazily<VirtualAccountSettingsEntity>(s => s.Id == account.Value.Id);

				async Task successAction()
				{
					var changed = await TrySetVirtualAccountSettingsBase<VirtualAccountSettingsEntity, SetVirtualAccountSettingsParameter>(settings, request.Parameter, response.Validation);

					if (changed)
					{
						Connection.Update(settings);
						Connection.SaveChanges();

						OnVirtualAccountSettingsChanged.Invoke(Session, settings.Value, settings.Value.CloneAsT());

						LogIfAccessingAsDelegate(user, "set account settings for " + account.Value.Name);
					}
				}

				await FirstValidateAsAccount(user, citizen, account, response.Validation)
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IResponse> SetDepartmentAccountSettings(IAsAccountEncryptableRequest<IEventfulAccountService.SetDepartmentAccountSettingsParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<DepartmentAccountEntity> account = GetAccountEntityLazily<DepartmentAccountEntity>(request);
				Lazy<DepartmentAccountSettingsEntity> settings = Connection.GetSingleLazily<DepartmentAccountSettingsEntity>(s => s.Id == account.Value.Id);

				async Task successAction()
				{
					var changed = await TrySetVirtualAccountSettingsBase<DepartmentAccountSettingsEntity, SetDepartmentAccountSettingsParameter>(settings, request.Parameter, response.Validation);

					if (changed)
					{
						Connection.Update(settings);
						Connection.SaveChanges();

						OnDepartmentAccountSettingsChanged.Invoke(Session, settings.Value, settings.Value.CloneAsT());

						LogIfAccessingAsDelegate(user, "set account settings for " + account.Value.Name);
					}
				}

				await FirstValidateAsAccount(user, citizen, account, response.Validation)
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<DepositAccountReferenceEntity>> GetDepositReferencesForReferencedAccount(IAsAccountRequest request)
		{
			var response = new GetPaginatedEncryptableResponse<DepositAccountReferenceEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<RealAccountEntity> account = GetAccountEntityLazily<RealAccountEntity>(request);

				void successAction()
				{
					response.Data = Connection.Query<DepositAccountReferenceEntity>()
						.Where(r => r.ReferencedAccount.Id == account.Value.Id)
						.Select(r => r.BasicClone())
						.ToList();

					LogIfAccessingAsDelegate(user, "retrieved deposit references for referenced account " + account.Value.Name);
				}

				await FirstValidateAsAccount(user, citizen, account, response.Validation)
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<DepositAccountReferenceEntity>> GetDepositReferencesForReferencingAccount(IAsAccountRequest request)
		{
			var response = new GetPaginatedEncryptableResponse<DepositAccountReferenceEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<IVirtualAccountEntity> account = GetAccountEntityLazily<IVirtualAccountEntity>(request);

				void successAction()
				{
					response.Data = Connection.Query<DepositAccountReferenceEntity>()
						.Select(r => r.BasicClone())
						.ToList();

					LogIfAccessingAsDelegate(user, "retrieved deposit references for referencing account " + account.Value.Name);
				}

				await FirstValidateAsAccount(user, citizen, account, response.Validation)
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IEncryptableResponse<RealAccountSettingsEntity>> GetRealAccountSettings(IAsAccountRequest request)
		{
			var response = new EncryptableResponse<RealAccountSettingsEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<RealAccountEntity> account = GetAccountEntityLazily<RealAccountEntity>(request);

				void successAction()
				{
					response.Overwrite(Connection.GetSingle<RealAccountSettingsEntity>(s => s.Owner.Id == account.Value.Id).CloneAsT());

					LogIfAccessingAsDelegate(user, "retrieved account settings for " + account.Value.Name);
				}

				await FirstValidateAsAccount(user, citizen, account, response.Validation)
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IEncryptableResponse<VirtualAccountSettingsEntity>> GetVirtualAccountSettings(IAsAccountRequest request)
		{
			var response = new EncryptableResponse<VirtualAccountSettingsEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<VirtualAccountEntity> account = GetAccountEntityLazily<VirtualAccountEntity>(request);

				void successAction()
				{
					response.Overwrite(Connection.GetSingle<VirtualAccountSettingsEntity>(s => s.Owner.Id == account.Value.Id).CloneAsT());

					LogIfAccessingAsDelegate(user, "retrieved account settings for " + account.Value.Name);
				}

				await FirstValidateAsAccount(user, citizen, account, response.Validation)
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IEncryptableResponse<DepartmentAccountSettingsEntity>> GetDepartmentAccountSettings(IAsAccountRequest request)
		{
			var response = new EncryptableResponse<DepartmentAccountSettingsEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<DepartmentAccountEntity> account = GetAccountEntityLazily<DepartmentAccountEntity>(request);

				void successAction()
				{
					response.Overwrite(Connection.GetSingle<DepartmentAccountSettingsEntity>(s => s.Owner.Id == account.Value.Id).CloneAsT());

					LogIfAccessingAsDelegate(user, "retrieved account settings for " + account.Value.Name);
				}

				await FirstValidateAsAccount(user, citizen, account, response.Validation)
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<IAccountEntity>> GetAccounts(IAsCitizenRequest request)
		{
			var response = new GetPaginatedEncryptableResponse<IAccountEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);

				void successAction()
				{
					response.Data = new List<IAccountEntity>()
					{
						Connection.GetSingle<RealAccountEntity>(r=>r.Owner.Id == citizen.Value.Id).CloneAsT()
					};
					Connection.Query<VirtualAccountEntity>()
						.Where(v => v.Owner.Id == citizen.Value.Id || v.Admins.Any(a => a.Id == citizen.Value.Id))
						.ForEach(v => response.Data.Add(v.CloneAsT()));
					Connection.Query<DepartmentAccountEntity>()
						.Where(d => d.Department.Admins.Any(a => a.Id == citizen.Value.Id))
						.ForEach(d => response.Data.Add(d.CloneAsT()));

					LogIfAccessingAsDelegate(user, "retrieved accounts for citizen " + citizen.Value.Name);
				}

				await FirstValidateAsCitizen(user, citizen, response.Validation)
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IResponse> EditDepositAccountReference(IAsAccountEncryptableRequest<EditDepositAccountReferenceParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<IVirtualAccountEntity> account = GetAccountEntityLazily<IVirtualAccountEntity>(request);
				Lazy<DepositAccountReferenceEntity> reference = Connection.GetSingleLazily<DepositAccountReferenceEntity>(request.Parameter.AccountReferenceId);

				async Task successAction()
				{
					Boolean changed = false;

					var tasks = new List<Task>();

					Boolean absoluteLimitCheck()
					{
						return reference.Value.AbsoluteLimit != request.Parameter.AbsoluteLimit.Value;
					}
					void absoluteLimitSuccessAction()
					{
						reference.Value.AbsoluteLimit = request.Parameter.AbsoluteLimit.Value;
						changed = true;
					}

					tasks.Add(FirstNullCheck(request.Parameter.AbsoluteLimit)
					 .NextCompound(absoluteLimitCheck)
					 .SetOnCriterionMet(absoluteLimitSuccessAction)
					 .Evaluate());

					Boolean relativeLimitCheck()
					{
						return reference.Value.RelativeLimit != request.Parameter.RelativeLimit.Value;
					}
					void relativeLimitSuccessAction()
					{
						reference.Value.RelativeLimit = request.Parameter.RelativeLimit.Value;
						changed = true;
					}

					tasks.Add(FirstNullCheck(request.Parameter.RelativeLimit)
					 .NextCompound(relativeLimitCheck)
					 .SetOnCriterionMet(relativeLimitSuccessAction)
					 .Evaluate());

					Boolean isActiveCheck()
					{
						return reference.Value.IsActive != request.Parameter.IsActive.Value;
					}
					void isActiveSuccessAction()
					{
						reference.Value.IsActive = request.Parameter.IsActive.Value;
						changed = true;
					}

					tasks.Add(FirstNullCheck(request.Parameter.IsActive)
					 .NextCompound(isActiveCheck)
					 .SetOnCriterionMet(isActiveSuccessAction)
					 .Evaluate());

					Boolean useAsForwardingCheck()
					{
						return reference.Value.UseAsForwarding != request.Parameter.UseAsForwarding.Value;
					}
					void useAsForwardingSuccessAction()
					{
						reference.Value.UseAsForwarding = request.Parameter.UseAsForwarding.Value;
						changed = true;
					}

					tasks.Add(FirstNullCheck(request.Parameter.UseAsForwarding)
					 .NextCompound(useAsForwardingCheck)
					 .SetOnCriterionMet(useAsForwardingSuccessAction)
					 .Evaluate());

					await Task.WhenAll(tasks);

					if (changed)
					{
						Connection.Update(reference);
						Connection.SaveChanges();

						OnDepositAccountReferenceChangedForReferenced.Invoke(Session, reference.Value.ReferencedAccount, reference.Value.BasicClone());
						OnDepositAccountReferenceChangedForReferencing.Invoke(Session, account.Value, reference.Value.AdvancedClone());

						LogIfAccessingAsDelegate(user, "edited deposit account reference for " + account.Value.Name + " referencing " + reference.Value.ReferencedAccount.Name);
					}
				}

				await FirstValidateAsAccount(user, citizen, account, response.Validation)
					   .NextManagerManagesProperty(account, reference, Connection, response.Validation.GetField(nameof(request.Parameter.AccountReferenceId)))
					   .SetOnCriterionMet(successAction)
					   .Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IResponse> CreateVirtualAccount(IAsCitizenRequest<CreateVirtualAccountParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<VirtualAccountEntity> duplicate = Connection.GetFirstLazily<VirtualAccountEntity>(v => v.Name.ToLower().Equals(request.Parameter.Name.ToLower()));

				void successAction()
				{
					CreditScoreEntity newCreditScore = new CreditScoreEntity();
					VirtualAccountEntity newAccount = new VirtualAccountEntity(citizen.Value, request.Parameter.Name, newCreditScore);

					List<CurrencyEntity> currencies = Connection.Query<CurrencyEntity>().Where(c => c.IsActive).ToList();

					var settings = new VirtualAccountSettingsEntity(newAccount,
													  new CurrencyBoolDictionaryEntity(currencies),
													  new CurrencyBoolDictionaryEntity(currencies),
													  new CurrencyBoolDictionaryEntity(currencies));
					Connection.Insert(newCreditScore, newAccount, settings.CanBeMiddlemanFor, settings.CanCreateTransactionOffersFor, settings.CanReceiveTransactionOffersFor, settings);
					Connection.SaveChanges();

					OnVirtualAccountCreated.Invoke(Session, citizen.Value, newAccount.CloneAsT());

					LogIfAccessingAsDelegate(user, "created virtual account " + newAccount.Name);
				}

				await FirstValidateAsCitizen(user, citizen, response.Validation)
					.NextNullCheck(duplicate.Value,
						response.Validation.GetField(nameof(request.Parameter.Name)),
						DefaultCode.Duplicate.SetMessage("A virtual account using this name has already been created."))
					.InvertCriterion()
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IResponse> CreateDepartmentAccount(IAsCitizenEncryptableRequest<CreateDepartmentAccountParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<DepartmentEntityBase> department = Connection.GetSingleLazily<DepartmentEntityBase>(request.Parameter.DepartmentId);
				Lazy<DepartmentAccountEntity> duplicate = Connection.GetFirstLazily<DepartmentAccountEntity>(v => v.Name.ToLower().Equals(request.Parameter.Name.ToLower()));

				void successAction()
				{
					CreditScoreEntity newCreditScore = new CreditScoreEntity();
					DepartmentAccountEntity newAccount = new DepartmentAccountEntity(citizen.Value, request.Parameter.Name, newCreditScore, department.Value);

					List<CurrencyEntity> currencies = Connection.Query<CurrencyEntity>().Where(c => c.IsActive).ToList();

					var settings = new DepartmentAccountSettingsEntity(newAccount,
													  new CurrencyBoolDictionaryEntity(currencies),
													  new CurrencyBoolDictionaryEntity(currencies),
													  new CurrencyBoolDictionaryEntity(currencies));
					Connection.Insert(newCreditScore, newAccount, settings.CanBeMiddlemanFor, settings.CanCreateTransactionOffersFor, settings.CanReceiveTransactionOffersFor, settings);
					Connection.SaveChanges();

					OnDepartmentAccountCreated.Invoke(Session, department.Value.Admins, newAccount.CloneAsT());

					LogIfAccessingAsDelegate(user, "created department account " + newAccount.Name);
				}

				await FirstValidateAsCitizen(user, citizen, response.Validation)
					.NextManagerManagesProperty(citizen, department, Connection, response.Validation.GetField(nameof(request.Parameter.DepartmentId)))
					.NextNullCheck(duplicate.Value,
						response.Validation.GetField(nameof(request.Parameter.Name)),
						DefaultCode.Duplicate.SetMessage("A department account using this name has already been created."))
					.InvertCriterion()
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IResponse> CreateDepositAccountReference(IAsAccountEncryptableRequest<CreateAccountReferenceParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<IVirtualAccountEntity> account = GetAccountEntityLazily<IVirtualAccountEntity>(request);
				Lazy<CurrencyEntity> currency = Connection.GetSingleLazily<CurrencyEntity>(request.Parameter.CurrencyId);
				Lazy<RealAccountEntity> referencedAccount = Connection.GetSingleLazily<RealAccountEntity>(request.Parameter.ReferencedAccountId);
				Lazy<DepositAccountReferenceEntity> duplicate = new Lazy<DepositAccountReferenceEntity>(() => account.Value.DepositReferences.SingleOrDefault(d => d.Id == referencedAccount.Value.Id && d.Currency.Id == currency.Value.Id));
				Lazy<RealAccountSettingsEntity> settings = Connection.GetFirstLazily<RealAccountSettingsEntity>(s => s.Owner.Id == referencedAccount.Value.Id);

				Boolean canBeDepositAccountForCheck()
				{
					return settings.Value.CanBeDepositAccountFor[currency.Value];
				}
				void successAction()
				{
					IVirtualAccountSettingsEntity virtualSettings = Connection.GetFirst<IVirtualAccountSettingsEntity>(s => s.Owner.Id == account.Value.Id);
					DepositAccountReferenceEntity newDepositReference = new DepositAccountReferenceEntity(referencedAccount.Value, currency.Value)
					{
						AbsoluteLimit = virtualSettings.DefaultDepositAccountMapAbsoluteLimit,
						RelativeLimit = virtualSettings.DefaultDepositAccountMapRelativeLimit
					};
					Connection.Insert(newDepositReference);
					account.Value.DepositReferences.Add(newDepositReference);
					Connection.Update(account.Value);
					Connection.SaveChanges();

					OnDepositAccountReferenceCreatedForReferenced.Invoke(Session, newDepositReference.ReferencedAccount, newDepositReference.BasicClone());
					OnDepositAccountReferenceCreatedForReferencing.Invoke(Session, account.Value, newDepositReference.AdvancedClone());

					LogIfAccessingAsDelegate(user, "created deposit account reference for " + account.Value.Name + " referencing " + newDepositReference.ReferencedAccount.Name);
				}

				await FirstValidateAsAccount(user, citizen, account, response.Validation)
					.NextNullCheck(currency.Value,
						response.Validation.GetField(nameof(request.Parameter.CurrencyId)),
						DefaultCode.NotFound.SetMessage("The currency requested could not be found."))
					.NextNullCheck(referencedAccount.Value,
						response.Validation.GetField(nameof(request.Parameter.ReferencedAccountId)),
						DefaultCode.NotFound.SetMessage("The account requested could not be found."))
					.NextNullCheck(duplicate.Value,
						DefaultCode.Duplicate.SetMessage("A reference using this account and currency has already been created."))
					.NextCompound(canBeDepositAccountForCheck,
						DefaultCode.Unauthorized.SetMessage("This account is already referenced."))
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IResponse> DeleteDepositAccountReference(IAsAccountEncryptableRequest<DeleteDepositAccountReferenceParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<AccountEntityBase> account = GetAccountEntityLazily<AccountEntityBase>(request);
				Lazy<VirtualAccountEntityBase> referencingAccount = Connection.GetFirstLazily<VirtualAccountEntityBase>(v => v.DepositReferences.Any(r => r.Id == request.Parameter.AccountReferenceId));
				Lazy<DepositAccountReferenceEntity> reference = new Lazy<DepositAccountReferenceEntity>(() => referencingAccount.Value.DepositReferences.Single(d => d.Id == request.Parameter.AccountReferenceId));

				async Task successAction()
				{
					IQueryable<SourceTransactionContractEntity> openTransactions = Connection
						.Query<SourceTransactionContractEntity>()
						.Where(s => !s.IsBooked &&
						 ((s.Relationship == TransactionPartnersRelationship.RealToVirtual &&
							 s.Creditor.Id == referencingAccount.Value.Id &&
							 s.TargetTransactionContracts.Any(t =>
								 !t.IsBooked &&
								 ((t.Creditor.Id == reference.Value.ReferencedAccount.Id) ||
								  (t.Debtor.Id == reference.Value.ReferencedAccount.Id && t.Relationship == TransactionPartnersRelationship.ForwardToDeposit)))) ||
						 (s.Relationship == TransactionPartnersRelationship.VirtualToReal &&
							 s.Debtor.Id == referencingAccount.Value.Id &&
							 s.TargetTransactionContracts.Any(t =>
								 !t.IsBooked &&
								 ((t.Creditor.Id == reference.Value.ReferencedAccount.Id && t.Relationship == TransactionPartnersRelationship.DepositToForward) ||
								  (t.Debtor.Id == reference.Value.ReferencedAccount.Id)))) ||
						 (s.Relationship == TransactionPartnersRelationship.VirtualToVirtual &&
							 (s.Debtor.Id == referencingAccount.Value.Id &&
							  s.TargetTransactionContracts.Any(t =>
								 !t.IsBooked &&
								 ((t.Creditor.Id == reference.Value.ReferencedAccount.Id && t.Relationship == TransactionPartnersRelationship.DepositToForward) ||
								  (t.Debtor.Id == reference.Value.ReferencedAccount.Id && (t.Relationship == TransactionPartnersRelationship.DepositToForward || t.Relationship == TransactionPartnersRelationship.ForwardToForward)))) ||
							 (s.Creditor.Id == referencingAccount.Value.Id &&
							  s.TargetTransactionContracts.Any(t =>
								 !t.IsBooked &&
								 ((t.Creditor.Id == reference.Value.ReferencedAccount.Id && (t.Relationship == TransactionPartnersRelationship.ForwardToDeposit || t.Relationship == TransactionPartnersRelationship.ForwardToForward)) ||
								  (t.Debtor.Id == reference.Value.ReferencedAccount.Id && t.Relationship == TransactionPartnersRelationship.ForwardToDeposit)))))) ||
						 (s.Relationship == TransactionPartnersRelationship.Equalizing &&
							 s.Debtor.Id == referencingAccount.Value.Id &&
							  s.TargetTransactionContracts.Any(t =>
								 !t.IsBooked &&
								 (t.Creditor.Id == reference.Value.ReferencedAccount.Id || t.Debtor.Id == reference.Value.ReferencedAccount.Id)))));

					referencingAccount.Value.DepositReferences.Remove(reference.Value);
					Boolean deletionPossible = reference.Value.AbsoluteBalance == 0;
					SourceTransactionContractEntity deletionTransaction = null;
					if (!deletionPossible)
					{
						var settings = Connection.GetSingle<RealAccountSettingsEntity>(s => s.Owner.Id == reference.Value.ReferencedAccount.Id);

						var transactionService = GetService<IEventfulTransactionService>();

						deletionTransaction = transactionService.CreateSourceTransactionContract(referencingAccount.Value,
																						reference.Value.ReferencedAccount,
																						account.Value,
																						account.Value.Id == reference.Value.ReferencedAccount.Id ? reference.Value.ReferencedAccount : (AccountEntityBase)referencingAccount.Value,
																					   reference.Value.AbsoluteBalance,
																					   reference.Value.Currency,
																					   CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																					   new List<TagEntity>(),
																					   settings.MinimumContractLifeSpan);

						Boolean targetsValid()
						{
							var states = deletionTransaction.TargetTransactionContracts.Select(t => transactionService.ValidateBookingValue(deletionTransaction, t, t.Relationship == TransactionPartnersRelationship.EqualizingDepositToForward ? t.Debtor : t.Creditor, t.Gross));
							return states.All(s => s);
						}

						deletionPossible = deletionTransaction != null && targetsValid();
					}

					Boolean openTransactionsCheck()
					{
						return !openTransactions.Any();
					}
					Boolean deletionPossibleCheck()
					{
						return deletionPossible;
					}
					void successAction()
					{
						Connection.Update(referencingAccount);
						Connection.Delete(reference.Value);
						if (deletionTransaction != null)
						{
							deletionTransaction.TargetTransactionContracts.ForEach(t => Connection.Insert(t));
							Connection.Insert(deletionTransaction);
						}
						Connection.SaveChanges();

						OnDepositAccountReferenceDeleted.Invoke(reference.Value);

						LogIfAccessingAsDelegate(user, "deleted deposit account reference");
					}

					await FirstCompound(openTransactionsCheck, response.Validation.GetField(DefaultField.MiscellaneousName), DefaultCode.Invalid.SetMessage("Due to open target transactions, this account reference cannot be deleted."))
						.NextCompound(deletionPossibleCheck, response.Validation.GetField(DefaultField.MiscellaneousName), DefaultCode.Invalid.SetMessage("It would be impossible to book the deposit account balance to this account."))
						.SetOnCriterionMet(successAction)
						.Evaluate();
				}

				await FirstValidateAsAccount(user, citizen, account, response.Validation)
					.NextNullCheck(referencingAccount.Value,
						response.Validation.GetField(nameof(request.Parameter.AccountReferenceId)),
						DefaultCode.NotFound.SetMessage("The account requested could not be found."))
					.NextManagerManagesProperty(account, reference, Connection, response.Validation.GetField(nameof(request.Parameter.AccountReferenceId)))
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		private async Task<IEnumerable<TAccount>> SearchAccounts<TAccount, TAccountSettings, TParameter>(IAsAccountGetPaginatedEncryptableRequest<TParameter> request, IValidationFieldCollection validation)
			where TAccount : IAccountEntity
			where TAccountSettings : IAccountSettingsEntity
			where TParameter : SearchAccountsParameterBase
		{
			var retVal1 = Connection.Query<TAccountSettings>();
			if (request.Parameter.Accessibility.HasValue && request.Parameter.Accessibility.Value == AccessibilityType.Private)
			{
				var user = GetUserEntity(request);
				Boolean userIsInRoleCheck()
				{
					return user.IsInRole(PBCommon.Settings.ADMIN_ROLE) || user.IsInRole(PBCommon.Settings.SUPERADMIN_ROLE);
				}
				void accessibilitySuccessAction()
				{
					retVal1 = retVal1.Where(s => s.Accessibility == request.Parameter.Accessibility.Value);
				}

				await FirstValidateAsUser(user, validation)
					.NextCompound(userIsInRoleCheck)
					.SetOnCriterionMet(accessibilitySuccessAction)
					.Evaluate();
			}
			else
			{
				retVal1 = retVal1.Where(s => s.Accessibility == AccessibilityType.Public);
			}
			if (request.Parameter.CanBeRecruitedIntoDepartments.HasValue)
			{
				retVal1 = retVal1.Where(s => s.CanBeRecruitedIntoDepartments == request.Parameter.CanBeRecruitedIntoDepartments.Value);
			}
			if (request.Parameter.ForcePriorityTags.HasValue)
			{
				retVal1 = retVal1.Where(s => s.ForcePriorityTags == request.Parameter.ForcePriorityTags.Value);
			}
			if (request.Parameter.MinimumContractLifeSpan.HasValue)
			{
				retVal1 = retVal1.Where(s => s.MinimumContractLifeSpan == request.Parameter.MinimumContractLifeSpan.Value);
			}
			if (request.Parameter.TransactionOfferLifetime.HasValue)
			{
				retVal1 = retVal1.Where(s => s.TransactionOfferLifetime == request.Parameter.TransactionOfferLifetime.Value);
			}
			if (request.Parameter.CanReceiveTransactionOffersFor?.Any() ?? false)
			{
				retVal1 = retVal1.Where(s => request.Parameter.CanReceiveTransactionOffersFor.All(kvp1 => s.CanReceiveTransactionOffersFor.Any(kvp2 => kvp2.Key.Id == kvp1.Key && kvp2.Value == kvp1.Value)));
			}
			if (request.Parameter.CanCreateTransactionOffersFor?.Any() ?? false)
			{
				retVal1 = retVal1.Where(s => request.Parameter.CanCreateTransactionOffersFor.All(kvp1 => s.CanCreateTransactionOffersFor.Any(kvp2 => kvp2.Key.Id == kvp1.Key && kvp2.Value == kvp1.Value)));
			}
			if (request.Parameter.CanBeMiddlemanFor?.Any() ?? false)
			{
				retVal1 = retVal1.Where(s => request.Parameter.CanBeMiddlemanFor.All(kvp1 => s.CanBeMiddlemanFor.Any(kvp2 => kvp2.Key.Id == kvp1.Key && kvp2.Value == kvp1.Value)));
			}
			IEnumerable<TAccount> retVal2 = retVal1.Select(s => Connection.GetSingle<TAccount>(s.Owner.Id));
			if (request.Parameter.Name != null)
			{
				var name = request.Parameter.Name.ToLower();
				retVal2 = retVal2.Where(a => a.Name.ToLower().Contains(name));
			}
			if (request.Parameter.ExcludeIds?.Any() ?? false)
			{
				retVal2 = retVal2.Where(a => !request.Parameter.ExcludeIds.Contains(a.Id));
			}
			if (request.Parameter.ExcludeNames?.Any() ?? false)
			{
				retVal2 = retVal2.Where(a => !request.Parameter.ExcludeNames.Contains(a.Name.ToLower()));
			}
			if (request.Parameter.CreatorId.HasValue)
			{
				retVal2 = retVal2.Where(a => a.Creator.Id == request.Parameter.CreatorId.Value);
			}
			if (request.Parameter.TagsIds?.Any() ?? false)
			{
				retVal2 = retVal2.Where(a => request.Parameter.TagsIds.All(id => a.Tags.Any(t => t.Id == id)));
			}
			if (request.Parameter.PriorityTagsIds?.Any() ?? false)
			{
				retVal2 = retVal2.Where(a => request.Parameter.PriorityTagsIds.All(id => a.PriorityTags.Any(t => t.Id == id)));
			}
			if (request.Parameter.MangerIds?.Any() ?? false)
			{
				var managers = request.Parameter.MangerIds.Select(id => Connection.GetSingle<IEntity>(id)).Where(m => m != null).ToList();
				retVal2 = retVal2.ToList().Where(a => managers.All(m => m.Manages<IEntity, IAccountEntity>(a, Connection)));
			}

			return retVal2;
		}

		public async Task<IGetPaginatedEncryptableResponse<IAccountEntity>> SearchAccounts(IAsAccountGetPaginatedEncryptableRequest<SearchAccountsParameterBase> request)
		{
			var response = new GetPaginatedEncryptableResponse<IAccountEntity>();

			async Task notNullRequest()
			{
				var data = await SearchAccounts<IAccountEntity, IAccountSettingsEntity, SearchAccountsParameterBase>(request, response.Validation);
				void setData()
				{
					response.LastPage = data.GetPageCount(request.PerPage) - 1;
					response.Data = data.Paginate(request.PerPage, request.Page).Select(a => a.CloneAsT()).ToList();
				}
				await CachedCriterionChain.Cache.Get()
					.ThisValidatePagination(request, data, response.Validation)
					.SetOnCriterionMet(setData)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<RealAccountEntity>> SearchRealAccounts(IAsAccountGetPaginatedEncryptableRequest<SearchRealAccountsParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<RealAccountEntity>();

			async Task notNullRequest()
			{
				var data = await SearchAccounts<RealAccountEntity, RealAccountSettingsEntity, SearchRealAccountsParameter>(request, response.Validation);
				if (request.Parameter.CanBeDepositAccountFor?.Any() ?? false)
				{
					data = data.Select(a => Connection.GetSingle<RealAccountSettingsEntity>(s => s.Owner.Id == a.Id))
						.Where(s => request.Parameter.CanBeDepositAccountFor.All(kvp1 => s.CanBeDepositAccountFor.Any(kvp2 => kvp2.Key.Id == kvp1.Key && kvp2.Value == kvp1.Value)))
						.Select(s => s.Owner as RealAccountEntity)
						.Where(a => a != null);
				}
				void setData()
				{
					response.LastPage = data.GetPageCount(request.PerPage) - 1;
					response.Data = data.Paginate(request.PerPage, request.Page).Select(a => a.CloneAsT()).ToList();
				}
				await CachedCriterionChain.Cache.Get()
					.ThisValidatePagination(request, data, response.Validation)
					.SetOnCriterionMet(setData)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		private async Task<IEnumerable<TAccount>> SearchVirtualAccounts<TAccount, TAccountSettings, TParameter>(IAsAccountGetPaginatedEncryptableRequest<TParameter> request, IValidationFieldCollection validation)
			where TAccount : class, IVirtualAccountEntity
			where TAccountSettings : IVirtualAccountSettingsEntity
			where TParameter : SearchVirtualAccountsParameterBase
		{
			var retVal = await SearchAccounts<TAccount, TAccountSettings, TParameter>(request, validation);
			if (request.Parameter.DepositForwardLifeSpan.HasValue)
			{
				retVal = retVal.Select(a => Connection.GetSingle<TAccountSettings>(s => s.Owner.Id == a.Id))
					.Where(s => s.DepositForwardLifeSpan == request.Parameter.DepositForwardLifeSpan)
					.Select(s => s.Owner as TAccount)
					.Where(a => a != null);
			}
			return retVal;
		}

		public async Task<IGetPaginatedEncryptableResponse<IVirtualAccountEntity>> SearchVirtualAccounts(IAsAccountGetPaginatedEncryptableRequest<SearchVirtualAccountsParameterBase> request)
		{
			var response = new GetPaginatedEncryptableResponse<IVirtualAccountEntity>();

			async Task notNullRequest()
			{
				var data = await SearchVirtualAccounts<IVirtualAccountEntity, IVirtualAccountSettingsEntity, SearchVirtualAccountsParameterBase>(request, response.Validation);
				void setData()
				{
					response.LastPage = data.GetPageCount(request.PerPage) - 1;
					response.Data = data.Paginate(request.PerPage, request.Page).Select(a => a.CloneAsT()).ToList();
				}
				await CachedCriterionChain.Cache.Get()
					.ThisValidatePagination(request, data, response.Validation)
					.SetOnCriterionMet(setData)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<VirtualAccountEntity>> SearchVirtualAccounts(IAsAccountGetPaginatedEncryptableRequest<SearchVirtualAccountsParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<VirtualAccountEntity>();

			async Task notNullRequest()
			{
				var data = await SearchVirtualAccounts<VirtualAccountEntity, VirtualAccountSettingsEntity, SearchVirtualAccountsParameter>(request, response.Validation);
				void setData()
				{
					response.LastPage = data.GetPageCount(request.PerPage) - 1;
					response.Data = data.Paginate(request.PerPage, request.Page).Select(a => a.CloneAsT()).ToList();
				}
				await CachedCriterionChain.Cache.Get()
					.ThisValidatePagination(request, data, response.Validation)
					.SetOnCriterionMet(setData)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<DepartmentAccountEntity>> SearchDepartmentAccounts(IAsAccountGetPaginatedEncryptableRequest<SearchDepartmentAccountsParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<DepartmentAccountEntity>();

			async Task notNullRequest()
			{
				var data = await SearchVirtualAccounts<DepartmentAccountEntity, DepartmentAccountSettingsEntity, SearchDepartmentAccountsParameter>(request, response.Validation);
				void setData()
				{
					response.LastPage = data.GetPageCount(request.PerPage) - 1;
					response.Data = data.Paginate(request.PerPage, request.Page).Select(a => a.CloneAsT()).ToList();
				}
				await CachedCriterionChain.Cache.Get()
					.ThisValidatePagination(request, data, response.Validation)
					.SetOnCriterionMet(setData)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public IVirtualAccountSettingsEntity UpdateVirtualAccountSettings(IVirtualAccountEntity account)
		{
			IVirtualAccountSettingsEntity settings = Connection.Query<IVirtualAccountSettingsEntity>().Where(s => s.Owner.Id == account.Id).Single();

			System.Collections.Generic.List<CurrencyEntity> currencies = Connection.Query<CurrencyEntity>().ToList();

			Boolean changed = false;

			currencies.ForEach(c =>
			{
				System.Collections.Generic.IEnumerable<DepositAccountReferenceEntity> activeDepositAccountMaps = account.DepositReferences.Where(r => r.Currency.Id == c.Id && r.IsActive);
				System.Collections.Generic.IEnumerable<DepositAccountReferenceEntity> activeForwardingAccountMaps = activeDepositAccountMaps.Where(r => r.UseAsForwarding);
				if (!activeForwardingAccountMaps.Any() || !activeDepositAccountMaps.Any())
				{
					changed = changed || settings.CanCreateTransactionOffersFor[c] || settings.CanReceiveTransactionOffersFor[c] || settings.CanBeMiddlemanFor[c];
					settings.CanCreateTransactionOffersFor[c] = false;
					settings.CanReceiveTransactionOffersFor[c] = false;
					settings.CanBeMiddlemanFor[c] = false;
				}
				else
				{
					changed = changed || !settings.CanCreateTransactionOffersFor[c];
					settings.CanCreateTransactionOffersFor[c] = true;
				}
			});
			Connection.Update(settings);
			Connection.SaveChanges();
			if (changed)
			{
				if (settings is VirtualAccountSettingsEntity virtualAccountSettings)
				{
					OnVirtualAccountSettingsChanged.Invoke(Session,
					   virtualAccountSettings,
					   virtualAccountSettings.CloneAsT());
				}
				else if (settings is DepartmentAccountSettingsEntity departmentAccountSettings)
				{
					OnDepartmentAccountSettingsChanged.Invoke(Session,
					   departmentAccountSettings,
					   departmentAccountSettings.CloneAsT());
				}
			}
			return settings;
		}

		public void UpdateDepositBalance(DepositAccountReferenceEntity depositAccountReference, Decimal value)
		{
			depositAccountReference.AbsoluteBalance += value;
			Connection.Update(depositAccountReference);
			Connection.SaveChanges();
			OnDepositBalanceUpdated.Invoke(Session, depositAccountReference, depositAccountReference.AbsoluteBalance);
		}

		public void UpdateDepositBalance(IVirtualAccountEntity account, RealAccountEntity mappedAccount, Decimal value)
		{
			UpdateDepositBalance(account.DepositReferences.Single(r => r.ReferencedAccount.Id == mappedAccount.Id), value);
		}
	}
}
