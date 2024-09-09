using CBApplication.Requests.Abstractions;
using CBApplication.Services.Abstractions;

using CBCommon.Enums;

using CBData.Abstractions;
using CBData.Entities;

namespace CBApplication.Services
{
    public class AccountService : CBService, IEventfulAccountService
	{
		public AccountService(IServiceContext serviceContext) : base(serviceContext)
		{
			Observe<IEventfulAccountService>(this);
		}

		public event ServiceEventHandler<ServiceEventArgs<RealAccountSettingsEntity>> OnRealAccountSettingsChanged;
		public event ServiceEventHandler<ServiceEventArgs<VirtualAccountSettingsEntity>> OnVirtualAccountSettingsChanged;
		public event ServiceEventHandler<ServiceEventArgs<DepositAccountReferenceEntity>> OnDepositAccountReferenceChangedForReferencing;
		public event ServiceEventHandler<ServiceEventArgs<DepositAccountReferenceEntity>> OnDepositAccountReferenceChangedForReferenced;
		public event ServiceEventHandler<ServiceEventArgs<VirtualAccountEntity>> OnVirtualAccountCreated;
		public event ServiceEventHandler<ServiceEventArgs<DepositAccountReferenceEntity>> OnDepositAccountReferenceCreatedForReferenced;
		public event ServiceEventHandler<ServiceEventArgs<DepositAccountReferenceEntity>> OnDepositAccountReferenceCreatedForReferencing;
		public event ServiceEventHandler<ServiceEventArgs> OnDepositAccountReferenceDeleted;
		public event ServiceEventHandler<ServiceEventArgs<Decimal>> OnDepositBalanceUpdated;

		private async Task<Boolean> TrySetCurrencyBoolDictionaryEntity(Lazy<CurrencyBoolDictionaryEntity> dict, IDictionary<Guid, Boolean> requestDict, IHasValidationFieldSet response)
		{
			Boolean changed = false;
			if (requestDict.Any())
			{
				foreach (var kvp in requestDict)
				{
					var currency = Connection.GetSingle<CurrencyEntity>(kvp.Key);
					Boolean check()
					{
						return dict.Value[currency] != kvp.Value;
					}
					void successAction()
					{
						dict.Value[currency] = kvp.Value;
						changed = true;
					}
					await FirstNullCheck(currency, ValidationField.Create(nameof(requestDict)), ValidationCode.PartiallyNotFound)
						.NextCompound(check)
						.SetOnCriterionMet(successAction)
						.Evaluate(response);
				}
			}
			return changed;
		}

		private async Task<Boolean> TrySetAccountSettingsBase<TSettings, TParameter>(TSettings settings, TParameter parameter, IHasValidationFieldSet response)
			where TSettings : IAccountSettingsEntity
			where TParameter : IAccountService.SetAccountSettingsParameterBase
		{
			var changed = false;

			var changedCanBeMiddleManFor = await TrySetCurrencyBoolDictionaryEntity(new Lazy<CurrencyBoolDictionaryEntity>(() => settings.CanBeMiddlemanFor), parameter.CanBeMiddlemanFor, response);
			var changedCanReceiveTransactionOffersFor = await TrySetCurrencyBoolDictionaryEntity(new Lazy<CurrencyBoolDictionaryEntity>(() => settings.CanReceiveTransactionOffersFor), parameter.CanReceiveTransactionOffersFor, response);

			changed = changedCanBeMiddleManFor || changedCanReceiveTransactionOffersFor;

			var tasks = new List<Task>();

			Boolean transactionOfferLifetimeCheck()
			{
				return settings.TransactionOfferLifetime != parameter.TransactionOfferLifetime.Value;
			}
			void transactionOfferLifetimeSuccessAction()
			{
				settings.TransactionOfferLifetime = parameter.TransactionOfferLifetime.Value;
				changed = true;
			}

			tasks.Add(FirstNullCheck(parameter.TransactionOfferLifetime)
				.NextCompound(transactionOfferLifetimeCheck)
				.SetOnCriterionMet(transactionOfferLifetimeSuccessAction)
				.Evaluate(response));

			Boolean minimumContractLifeSpanCheck()
			{
				return settings.MinimumContractLifeSpan != parameter.MinimumContractLifeSpan.Value;
			}
			void minimumContractLifeSpanSuccessAction()
			{
				settings.MinimumContractLifeSpan = parameter.MinimumContractLifeSpan.Value;
				changed = true;
			}

			tasks.Add(FirstNullCheck(parameter.MinimumContractLifeSpan)
				.NextCompound(minimumContractLifeSpanCheck)
				.SetOnCriterionMet(minimumContractLifeSpanSuccessAction)
				.Evaluate(response)); ;

			Boolean canBeRecruitedIntoDepartmentsCheck()
			{
				return settings.CanBeRecruitedIntoDepartments != parameter.CanBeRecruitedIntoDepartments.Value;
			}
			void canBeRecruitedIntoDepartmentsCheckSuccessAction()
			{
				settings.CanBeRecruitedIntoDepartments = parameter.CanBeRecruitedIntoDepartments.Value;
				changed = true;
			}

			tasks.Add(FirstNullCheck(parameter.CanBeRecruitedIntoDepartments)
				.NextCompound(canBeRecruitedIntoDepartmentsCheck)
				.SetOnCriterionMet(canBeRecruitedIntoDepartmentsCheckSuccessAction)
				.Evaluate(response));

			Boolean forcePriorityTagsCheck()
			{
				return settings.ForcePriorityTags != parameter.ForcePriorityTags.Value;
			}
			void forcePriorityTagsCheckSuccessAction()
			{
				settings.ForcePriorityTags = parameter.ForcePriorityTags.Value;
				changed = true;
			}

			tasks.Add(FirstNullCheck(parameter.ForcePriorityTags)
				.NextCompound(forcePriorityTagsCheck)
				.SetOnCriterionMet(forcePriorityTagsCheckSuccessAction)
				.Evaluate(response));

			Boolean accessibilityCheck()
			{
				return settings.Accessibility != parameter.Accessibility.Value;
			}
			void accessibilitySuccessAction()
			{
				settings.Accessibility = parameter.Accessibility.Value;
				changed = true;
			}

			tasks.Add(FirstNullCheck(parameter.Accessibility)
				.NextCompound(accessibilityCheck)
				.SetOnCriterionMet(accessibilitySuccessAction)
				.Evaluate(response));

			await Task.WhenAll(tasks);

			return changed;
		}

		public async Task<IResponse> SetRealAccountSettings(IAsAccountEncryptableRequest<IAccountService.SetRealAccountSettingsParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				async Task successAction()
				{
					var account = GetAccountEntity<RealAccountEntity>(request);
					var settings = GetSettings<RealAccountSettingsEntity>(account);

					Boolean changed = await TrySetAccountSettingsBase<RealAccountSettingsEntity, IAccountService.SetRealAccountSettingsParameter>(settings, request.Parameter, response);
					Boolean changedCanBeMiddleManFor = await TrySetCurrencyBoolDictionaryEntity(new Lazy<CurrencyBoolDictionaryEntity>(() => settings.CanBeMiddlemanFor), request.Parameter.CanBeDepositAccountFor, response);

					changed = changed || changedCanBeMiddleManFor;

					if (changed)
					{
						Connection.Update(settings);
						Connection.SaveChanges();

						OnRealAccountSettingsChanged.Invoke(Session, settings, settings.CloneAsT());

						LogIfAccessingAsDelegate(GetUserEntity(request), "set account settings for :{0}", account.Name);
					}
				}

				await FirstValidateAsAccount(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public async Task<IResponse> SetVirtualAccountSettings(IAsAccountEncryptableRequest<IAccountService.SetVirtualAccountSettingsParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				async Task successAction()
				{
					var account = GetAccountEntity<VirtualAccountEntity>(request);
					var settings = GetSettings<VirtualAccountSettingsEntity>(account);

					Boolean changed = await TrySetAccountSettingsBase<VirtualAccountSettingsEntity, IAccountService.SetVirtualAccountSettingsParameter>(settings, request.Parameter, response);

					var tasks = new List<Task>();

					Boolean depositForwardLifeSpanCheck()
					{
						return settings.DepositForwardLifeSpan != request.Parameter.DepositForwardLifeSpan.Value;
					}
					void depositForwardLifeSpanSuccessAction()
					{
						settings.DepositForwardLifeSpan = request.Parameter.DepositForwardLifeSpan.Value;
						changed = true;
					}

					tasks.Add(FirstNullCheck(request.Parameter.DepositForwardLifeSpan)
						.NextCompound(depositForwardLifeSpanCheck)
						.SetOnCriterionMet(depositForwardLifeSpanSuccessAction)
						.Evaluate(response));

					Boolean defaultDepositAccountMapRelativeLimitCheck()
					{
						return settings.DefaultDepositAccountMapRelativeLimit != request.Parameter.DefaultDepositAccountMapRelativeLimit.Value;
					}
					void defaultDepositAccountMapRelativeLimitSuccessAction()
					{
						settings.DefaultDepositAccountMapRelativeLimit = request.Parameter.DefaultDepositAccountMapRelativeLimit.Value;
						changed = true;
					}

					tasks.Add(FirstNullCheck(request.Parameter.DefaultDepositAccountMapRelativeLimit)
						.NextCompound(defaultDepositAccountMapRelativeLimitCheck)
						.SetOnCriterionMet(defaultDepositAccountMapRelativeLimitSuccessAction)
						.Evaluate(response));

					Boolean defaultDepositAccountMapAbsoluteLimitCheck()
					{
						return settings.DefaultDepositAccountMapAbsoluteLimit != request.Parameter.DefaultDepositAccountMapAbsoluteLimit.Value;
					}
					void defaultDepositAccountMapAbsoluteLimitSuccessAction()
					{
						settings.DefaultDepositAccountMapAbsoluteLimit = request.Parameter.DefaultDepositAccountMapAbsoluteLimit.Value;
						changed = true;
					}

					tasks.Add(FirstNullCheck(request.Parameter.DefaultDepositAccountMapAbsoluteLimit)
						.NextCompound(defaultDepositAccountMapAbsoluteLimitCheck)
						.SetOnCriterionMet(defaultDepositAccountMapAbsoluteLimitSuccessAction)
						.Evaluate(response));

					await Task.WhenAll(tasks);
					if (changed)
					{
						Connection.Update(settings);
						Connection.SaveChanges();

						OnVirtualAccountSettingsChanged.Invoke(Session, settings, settings.CloneAsT());

						LogIfAccessingAsDelegate(GetUserEntity(request), "set account settings for :{0}", account.Name);
					}
				}

				await FirstValidateAsAccount(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<DepositAccountReferenceEntity>> GetDepositReferencesForReferencedAccount(IAsAccountRequest request)
		{
			var response = new GetPaginatedEncryptableResponse<DepositAccountReferenceEntity>();

			async Task notNullRequest()
			{
				void successAction()
				{
					var account = GetAccountEntity<RealAccountEntity>(request);

					response.Data = Connection.Query<DepositAccountReferenceEntity>()
						.Where(r => r.ReferencedAccount.Id == account.Id)
						.Select(r => r.BasicClone())
						.ToList();

					LogIfAccessingAsDelegate(GetUserEntity(request), "retrieved deposit references for referenced account :{0}", account.Name);
				}

				await FirstValidateAsAccount(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<DepositAccountReferenceEntity>> GetDepositReferencesForReferencingAccount(IAsAccountRequest request)
		{
			var response = new GetPaginatedEncryptableResponse<DepositAccountReferenceEntity>();

			async Task notNullRequest()
			{
				void successAction()
				{
					VirtualAccountEntity account = GetAccountEntity<VirtualAccountEntity>(request);

					response.Data = account.DepositReferences
						.Select(r => r.AdvancedClone())
						.ToList();

					LogIfAccessingAsDelegate(GetUserEntity(request), "retrieved deposit references for referencing account :{0}", account.Name);
				}

				await FirstValidateAsAccount(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public async Task<IEncryptableResponse<RealAccountSettingsEntity>> GetRealAccountSettings(IAsAccountRequest request)
		{
			var response = new EncryptableResponse<RealAccountSettingsEntity>();

			async Task notNullRequest()
			{
				void successAction()
				{
					var account = GetAccountEntity<RealAccountEntity>(request);
					var settings = GetSettings<RealAccountSettingsEntity>(account);

					response.Overwrite(settings.CloneAsT());

					LogIfAccessingAsDelegate(GetUserEntity(request), "retrieved account settings for :{0}", account.Name);
				}

				await FirstValidateAsAccount(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public async Task<IEncryptableResponse<VirtualAccountSettingsEntity>> GetVirtualAccountSettings(IAsAccountRequest request)
		{
			var response = new EncryptableResponse<VirtualAccountSettingsEntity>();

			async Task notNullRequest()
			{
				void successAction()
				{
					var account = GetAccountEntity<VirtualAccountEntity>(request);
					var settings = GetSettings<VirtualAccountSettingsEntity>(account);

					response.Overwrite(settings.CloneAsT());

					LogIfAccessingAsDelegate(GetUserEntity(request), "retrieved account settings for :{0}", account.Name);
				}

				await FirstValidateAsAccount(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<IAccountEntity>> GetAccounts(IAsCitizenRequest request)
		{
			var response = new GetPaginatedEncryptableResponse<IAccountEntity>();

			async Task notNullRequest()
			{
				void successAction()
				{
					CitizenEntity citizen = GetCitizenEntity(request);

					response.Data = new List<IAccountEntity>()
						.Concat(citizen.GetHeldOwnerClaimsValues<RealAccountEntity>(Connection))
						.Concat(citizen.GetHeldAdminClaimsValuesRecursively<VirtualAccountEntity>(Connection))
						.Concat(citizen.GetHeldOwnerClaimsValuesRecursively<VirtualAccountEntity>(Connection))
						.Distinct()
						.ToArray();

					LogIfAccessingAsDelegate(GetUserEntity(request), "retrieved accounts for citizen :{0}", citizen.Name);
				}

				await FirstValidateAsCitizen(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public async Task<IResponse> EditDepositAccountReference(IAsAccountEncryptableRequest<IAccountService.EditDepositAccountReferenceParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				async Task validated()
				{
					var account = GetAccountEntity<VirtualAccountEntity>(request);
					var reference = account.DepositReferences.SingleOrDefault(r => r.Id == request.Parameter.AccountReferenceId);

					async Task successAction()
					{
						Boolean changed = false;

						var tasks = new List<Task>();

						Boolean absoluteLimitCheck()
						{
							return reference.AbsoluteLimit != request.Parameter.AbsoluteLimit.Value;
						}
						void absoluteLimitSuccessAction()
						{
							reference.AbsoluteLimit = request.Parameter.AbsoluteLimit.Value;
							changed = true;
						}

						tasks.Add(FirstNullCheck(request.Parameter.AbsoluteLimit)
						 .NextCompound(absoluteLimitCheck)
						 .SetOnCriterionMet(absoluteLimitSuccessAction)
						 .Evaluate(response));

						Boolean relativeLimitCheck()
						{
							return reference.RelativeLimit != request.Parameter.RelativeLimit.Value;
						}
						void relativeLimitSuccessAction()
						{
							reference.RelativeLimit = request.Parameter.RelativeLimit.Value;
							changed = true;
						}

						tasks.Add(FirstNullCheck(request.Parameter.RelativeLimit)
						 .NextCompound(relativeLimitCheck)
						 .SetOnCriterionMet(relativeLimitSuccessAction)
						 .Evaluate(response));

						Boolean isActiveCheck()
						{
							return reference.IsActive != request.Parameter.IsActive.Value;
						}
						void isActiveSuccessAction()
						{
							reference.IsActive = request.Parameter.IsActive.Value;
							changed = true;
						}

						tasks.Add(FirstNullCheck(request.Parameter.IsActive)
						 .NextCompound(isActiveCheck)
						 .SetOnCriterionMet(isActiveSuccessAction)
						 .Evaluate(response));

						Boolean useAsForwardingCheck()
						{
							return reference.UseAsForwarding != request.Parameter.UseAsForwarding.Value;
						}
						void useAsForwardingSuccessAction()
						{
							reference.UseAsForwarding = request.Parameter.UseAsForwarding.Value;
							changed = true;
						}

						tasks.Add(FirstNullCheck(request.Parameter.UseAsForwarding)
						 .NextCompound(useAsForwardingCheck)
						 .SetOnCriterionMet(useAsForwardingSuccessAction)
						 .Evaluate(response));

						await Task.WhenAll(tasks);

						if (changed)
						{
							Connection.Update(reference);
							Connection.SaveChanges();

							OnDepositAccountReferenceChangedForReferenced.Invoke(Session, reference.ReferencedAccount, reference.BasicClone());
							OnDepositAccountReferenceChangedForReferencing.Invoke(Session, account, reference.AdvancedClone());

							LogIfAccessingAsDelegate(GetUserEntity(request), "edited deposit account reference for {0} referencing {1}.", account.Name, reference.ReferencedAccount.Name);
						}
					}

					await FirstNullCheck(reference,
						ValidationField.Create(nameof(request.Parameter.AccountReferenceId)),
						ValidationCode.NotFound.WithMessage("The deposit account reference provided could not be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
				}

				await FirstValidateAsAccount(request, response)
					   .SetOnCriterionMet(validated)
					   .Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public async Task<IResponse> CreateVirtualAccount(IAsCitizenRequest<IAccountService.CreateVirtualAccountParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				VirtualAccountEntity duplicate = Connection.GetFirst<VirtualAccountEntity>(v => v.Name.ToLower().Equals(request.Parameter.Name.ToLower()));
				IDepartmentEntity department = Connection.GetSingle<IDepartmentEntity>(request.Parameter.DepartmentId);

				Boolean departmentCheck()
				{
					return request.Parameter.DepartmentId == Guid.Empty || department != null;
				}

				void successAction()
				{
					var citizen = GetCitizenEntity(request);

					var newCreditScore = new CreditScoreEntity();
					var newAccount = new VirtualAccountEntity(citizen, request.Parameter.Name, newCreditScore);

					var currencies = Connection.Query<CurrencyEntity>().Where(c => c.IsActive).ToList();

					var settings = new VirtualAccountSettingsEntity(new CurrencyBoolDictionaryEntity(currencies),
													  new CurrencyBoolDictionaryEntity(currencies),
													  new CurrencyBoolDictionaryEntity(currencies));

					Connection.Insert(newCreditScore, newAccount, settings.CanBeMiddlemanFor, settings.CanCreateTransactionOffersFor, settings.CanReceiveTransactionOffersFor, settings);
					Connection.SaveChanges();

					var claimService = GetService<IEventfulClaimService>();

					claimService.EnsureClaim(out var _, newAccount, new HashSet<IClaimService.EnsureRightDatum>() { new(PBCommon.Configuration.Settings.OwnerRight, true) }, settings);

					if (department != null)
					{
						claimService.EnsureClaim(department, newAccount, new IClaimService.EnsureRightDatum(PBCommon.Configuration.Settings.AdminRight, true));
						OnVirtualAccountCreated.Invoke(Session, citizen, newAccount.CloneAsT());
					}
					else
					{
						claimService.EnsureClaim(citizen, newAccount, new IClaimService.EnsureRightDatum(PBCommon.Configuration.Settings.OwnerRight, true));
						OnVirtualAccountCreated.Invoke(Session, citizen, newAccount.CloneAsT());
					}

					LogIfAccessingAsDelegate(GetUserEntity(request), "created virtual account {0}", newAccount.Name);
				}

				await FirstValidateAsCitizen(request, response)
					.NextNullCheck(duplicate,
						ValidationField.Create(nameof(request.Parameter.Name)),
						ValidationCode.Duplicate.WithMessage("A virtual account using this name has already been created."))
					.InvertCriterion()
					.NextCompound(departmentCheck,
						ValidationField.Create(nameof(request.Parameter.DepartmentId)),
						ValidationCode.NotFound.WithMessage("The department provided could not be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public async Task<IResponse> CreateDepositAccountReference(IAsAccountEncryptableRequest<IAccountService.CreateAccountReferenceParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				var currency = Connection.GetSingle<CurrencyEntity>(request.Parameter.CurrencyId);
				var referencedAccount = Connection.GetSingle<RealAccountEntity>(request.Parameter.ReferencedAccountId);

				async Task foundCurrencyAndAccount()
				{
					var account = GetAccountEntity<VirtualAccountEntity>(request);
					var duplicate = account.DepositReferences.SingleOrDefault(d => d.ReferencedAccount.Id == referencedAccount.Id && d.Currency.Id == currency.Id);
					var referencedAccountSettings = GetSettings<RealAccountSettingsEntity>(referencedAccount);

					Boolean canBeDepositAccountForCheck()
					{
						return referencedAccountSettings.CanBeDepositAccountFor[currency];
					}
					void successAction()
					{
						var virtualSettings = GetSettings<VirtualAccountSettingsEntity>(account);

						DepositAccountReferenceEntity newDepositReference = new(referencedAccount, currency)
						{
							AbsoluteLimit = virtualSettings.DefaultDepositAccountMapAbsoluteLimit,
							RelativeLimit = virtualSettings.DefaultDepositAccountMapRelativeLimit
						};
						Connection.Insert(newDepositReference);
						account.DepositReferences.Add(newDepositReference);
						Connection.Update(account);
						Connection.SaveChanges();

						OnDepositAccountReferenceCreatedForReferenced.Invoke(Session, newDepositReference.ReferencedAccount, newDepositReference.BasicClone());
						OnDepositAccountReferenceCreatedForReferencing.Invoke(Session, account, newDepositReference.AdvancedClone());

						LogIfAccessingAsDelegate(GetUserEntity(request), "created deposit account reference for {0} referencing {1}", account.Name, newDepositReference.ReferencedAccount.Name);
					}

					await FirstNullCheck(duplicate,
							ValidationField.Create(nameof(request.Parameter.ReferencedAccountId)),
							ValidationCode.Duplicate.WithMessage("A reference using this account and currency has already been created."))
						.NextCompound(canBeDepositAccountForCheck,
							ValidationCode.Unauthorized.WithMessage("This account may not be referenced for this currency."))
						.InheritField()
						.SetOnCriterionMet(successAction)
						.Evaluate(response);
				}

				await FirstValidateAsAccount(request, response)
					.NextNullCheck(currency,
						ValidationField.Create(nameof(request.Parameter.CurrencyId)),
						ValidationCode.NotFound.WithMessage("The currency requested could not be found."))
					.NextNullCheck(referencedAccount,
						ValidationField.Create(nameof(request.Parameter.ReferencedAccountId)),
						ValidationCode.NotFound.WithMessage("The account requested could not be found."))
					.SetOnCriterionMet(foundCurrencyAndAccount)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public async Task<IResponse> DeleteDepositAccountReference(IAsAccountEncryptableRequest<IAccountService.DeleteDepositAccountReferenceParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				VirtualAccountEntity account = GetAccountEntity<VirtualAccountEntity>(request);
				VirtualAccountEntity referencingAccount = Connection.GetFirst<VirtualAccountEntity>(v => v.DepositReferences.Any(r => r.Id == request.Parameter.AccountReferenceId));

				async Task successAction()
				{
					DepositAccountReferenceEntity reference = referencingAccount.DepositReferences.Single(d => d.Id == request.Parameter.AccountReferenceId);

					IQueryable<SourceTransactionContractEntity> openTransactions = Connection
						.Query<SourceTransactionContractEntity>()
						.Where(s => !s.IsBooked &&
						 ((s.Relationship == CitizenBankEnums.TransactionPartnersRelationship.RealToVirtual &&
							 s.Creditor.Id == referencingAccount.Id &&
							 s.TargetTransactionContracts.Any(t =>
								 !t.IsBooked &&
								 ((t.Creditor.Id == reference.ReferencedAccount.Id) ||
								  (t.Debtor.Id == reference.ReferencedAccount.Id && t.Relationship == CitizenBankEnums.TransactionPartnersRelationship.ForwardToDeposit)))) ||
						 (s.Relationship == CitizenBankEnums.TransactionPartnersRelationship.VirtualToReal &&
							 s.Debtor.Id == referencingAccount.Id &&
							 s.TargetTransactionContracts.Any(t =>
								 !t.IsBooked &&
								 ((t.Creditor.Id == reference.ReferencedAccount.Id && t.Relationship == CitizenBankEnums.TransactionPartnersRelationship.DepositToForward) ||
								  (t.Debtor.Id == reference.ReferencedAccount.Id)))) ||
						 (s.Relationship == CitizenBankEnums.TransactionPartnersRelationship.VirtualToVirtual &&
							 (s.Debtor.Id == referencingAccount.Id &&
							  s.TargetTransactionContracts.Any(t =>
								 !t.IsBooked &&
								 ((t.Creditor.Id == reference.ReferencedAccount.Id && t.Relationship == CitizenBankEnums.TransactionPartnersRelationship.DepositToForward) ||
								  (t.Debtor.Id == reference.ReferencedAccount.Id && (t.Relationship == CitizenBankEnums.TransactionPartnersRelationship.DepositToForward || t.Relationship == CitizenBankEnums.TransactionPartnersRelationship.ForwardToForward)))) ||
							 (s.Creditor.Id == referencingAccount.Id &&
							  s.TargetTransactionContracts.Any(t =>
								 !t.IsBooked &&
								 ((t.Creditor.Id == reference.ReferencedAccount.Id && (t.Relationship == CitizenBankEnums.TransactionPartnersRelationship.ForwardToDeposit || t.Relationship == CitizenBankEnums.TransactionPartnersRelationship.ForwardToForward)) ||
								  (t.Debtor.Id == reference.ReferencedAccount.Id && t.Relationship == CitizenBankEnums.TransactionPartnersRelationship.ForwardToDeposit)))))) ||
						 (s.Relationship == CitizenBankEnums.TransactionPartnersRelationship.Equalizing &&
							 s.Debtor.Id == referencingAccount.Id &&
							  s.TargetTransactionContracts.Any(t =>
								 !t.IsBooked &&
								 (t.Creditor.Id == reference.ReferencedAccount.Id || t.Debtor.Id == reference.ReferencedAccount.Id)))));

					referencingAccount.DepositReferences.Remove(reference);
					Boolean deletionPossible = reference.AbsoluteBalance == 0;
					SourceTransactionContractEntity deletionTransaction = null;
					if (!deletionPossible)
					{
						var settings = GetSettings<RealAccountSettingsEntity>(reference.ReferencedAccount);
						var transactionService = GetService<IEventfulTransactionService>();

						deletionTransaction = transactionService.CreateSourceTransactionContract(referencingAccount,
																						reference.ReferencedAccount,
																						account,
																						account.Id == reference.ReferencedAccount.Id ? reference.ReferencedAccount : referencingAccount.As<AccountEntityBase>(Connection),
																						reference.AbsoluteBalance,
																						reference.Currency,
																						CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																						new List<TagEntity>(),
																						settings.MinimumContractLifeSpan);

						Boolean targetsValid()
						{
							var states = deletionTransaction.TargetTransactionContracts.Select(t => transactionService.ValidateBookingValue(deletionTransaction, t, t.Relationship == CitizenBankEnums.TransactionPartnersRelationship.EqualizingDepositToForward ? t.Debtor : t.Creditor, t.Gross));
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
						Connection.Delete(reference);
						if (deletionTransaction != null)
						{
							deletionTransaction.TargetTransactionContracts.ForEach(t => Connection.Insert(t));
							Connection.Insert(deletionTransaction);
						}
						Connection.SaveChanges();

						OnDepositAccountReferenceDeleted.Invoke(reference);

						LogIfAccessingAsDelegate(GetUserEntity(request), "deleted deposit account reference for {0} referencing {1}", account.Name, reference.ReferencedAccount.Name);
					}

					await FirstCompound(openTransactionsCheck, ValidationField.Miscellaneous, ValidationCode.Invalid.WithMessage("Due to open target transactions, this account reference cannot be deleted."))
						.NextCompound(deletionPossibleCheck, ValidationField.Miscellaneous, ValidationCode.Invalid.WithMessage("It would be impossible to book the deposit account balance to this account."))
						.SetOnCriterionMet(successAction)
						.Evaluate(response);
				}

				await FirstValidateAsAccount(request, response)
					.NextNullCheck(referencingAccount,
						ValidationField.Create(nameof(request.Parameter.AccountReferenceId)),
						ValidationCode.NotFound.WithMessage("The account reference requested could not be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		private async Task<IEnumerable<(TAccount Account, TAccountSettings Settings)>> SearchAccounts<TAccount, TAccountSettings, TParameter>(IAsAccountGetPaginatedEncryptableRequest<TParameter> request, IResponse response)
			where TAccount : IAccountEntity
			where TAccountSettings : IAccountSettingsEntity
			where TParameter : IAccountService.SearchAccountsParameterBase
		{
			//Sort by account first, since most common search parameter will likely be name
			//Since results have to be instanciated (.ToList()), most of the result set should have been filtered beforehand, so as not to query too many datasets.

			IEnumerable<TAccount> retVal1 = Connection.Query<TAccount>();
			if (request.Parameter.Name != null)
			{
				var name = request.Parameter.Name.ToLower();
				retVal1 = retVal1.Where(a => a.Name.ToLower().Contains(name));
			}
			if (request.Parameter.ExcludeIds?.Any() ?? false)
			{
				retVal1 = retVal1.Where(a => !request.Parameter.ExcludeIds.Contains(a.Id));
			}
			if (request.Parameter.ExcludeNames?.Any() ?? false)
			{
				var excludeNames = request.Parameter.ExcludeNames.Select(n => n.ToLower()).ToArray();
				retVal1 = retVal1.Where(a => !excludeNames.Contains(a.Name.ToLower()));
			}
			if (request.Parameter.CreatorId.HasValue)
			{
				retVal1 = retVal1.Where(a => a.Creator.Id == request.Parameter.CreatorId.Value);
			}
			if (request.Parameter.TagsIds?.Any() ?? false)
			{
				retVal1 = retVal1.Where(a => request.Parameter.TagsIds.All(id => a.Tags.Any(t => t.Id == id)));
			}
			if (request.Parameter.PriorityTagsIds?.Any() ?? false)
			{
				retVal1 = retVal1.Where(a => request.Parameter.PriorityTagsIds.All(id => a.PriorityTags.Any(t => t.Id == id)));
			}

			retVal1 = retVal1.ToList();

			var retVal2 = retVal1.Select(a => (Account: a, Settings: GetSettings<TAccountSettings>(a)));

			if (request.Parameter.Accessibility.HasValue && request.Parameter.Accessibility.Value == AccessibilityType.Private)
			{
				var user = GetUserEntity(request);
				Boolean userIsInRoleCheck()
				{
					return user.HoldsAdminRight(Connection) || user.HoldsOwnerRight(Connection);
				}
				void isAdminOrOwner()
				{
					retVal2 = retVal2.Where(t => t.Settings.Accessibility == request.Parameter.Accessibility.Value);
				}
				void isNeitherAdminNorOwner()
				{
					retVal2 = retVal2.Where(t => t.Settings.Accessibility == request.Parameter.Accessibility.Value && (user.HoldsOwnerRightRecursively(Connection, t.Account) || user.HoldsAdminRightRecursively(Connection, t.Account) || user.HoldsObserverRightRecursively(Connection, t.Account)));
				}

				await FirstValidateAuthenticatedDelegate(request, response)
					.NextCompound(userIsInRoleCheck)
					.SetOnCriterionMet(isAdminOrOwner)
					.SetOnCriterionFailed(isNeitherAdminNorOwner)
					.Evaluate(response);
			}
			else
			{
				retVal2 = retVal2.Where(t => t.Settings.Accessibility == AccessibilityType.Public);
			}

			if (request.Parameter.CanBeRecruitedIntoDepartments.HasValue)
			{
				retVal2 = retVal2.Where(t => t.Settings.CanBeRecruitedIntoDepartments == request.Parameter.CanBeRecruitedIntoDepartments.Value);
			}
			if (request.Parameter.ForcePriorityTags.HasValue)
			{
				retVal2 = retVal2.Where(t => t.Settings.ForcePriorityTags == request.Parameter.ForcePriorityTags.Value);
			}
			if (request.Parameter.MinimumContractLifeSpan.HasValue)
			{
				retVal2 = retVal2.Where(t => t.Settings.MinimumContractLifeSpan == request.Parameter.MinimumContractLifeSpan.Value);
			}
			if (request.Parameter.TransactionOfferLifetime.HasValue)
			{
				retVal2 = retVal2.Where(t => t.Settings.TransactionOfferLifetime == request.Parameter.TransactionOfferLifetime.Value);
			}
			if (request.Parameter.CanReceiveTransactionOffersFor?.Any() ?? false)
			{
				retVal2 = retVal2.Where(t => request.Parameter.CanReceiveTransactionOffersFor.All(kvp1 => t.Settings.CanReceiveTransactionOffersFor.Any(kvp2 => kvp2.Key.Id == kvp1.Key && kvp2.Value == kvp1.Value)));
			}
			if (request.Parameter.CanCreateTransactionOffersFor?.Any() ?? false)
			{
				retVal2 = retVal2.Where(t => request.Parameter.CanCreateTransactionOffersFor.All(kvp1 => t.Settings.CanCreateTransactionOffersFor.Any(kvp2 => kvp2.Key.Id == kvp1.Key && kvp2.Value == kvp1.Value)));
			}
			if (request.Parameter.CanBeMiddlemanFor?.Any() ?? false)
			{
				retVal2 = retVal2.Where(t => request.Parameter.CanBeMiddlemanFor.All(kvp1 => t.Settings.CanBeMiddlemanFor.Any(kvp2 => kvp2.Key.Id == kvp1.Key && kvp2.Value == kvp1.Value)));
			}

			return retVal2;
		}

		public async Task<IGetPaginatedEncryptableResponse<IAccountEntity>> SearchAccounts(IAsAccountGetPaginatedEncryptableRequest<IAccountService.SearchAccountsParameterBase> request)
		{
			var response = new GetPaginatedEncryptableResponse<IAccountEntity>();

			async Task notNullRequest()
			{
				var data = await SearchAccounts<IAccountEntity, IAccountSettingsEntity, IAccountService.SearchAccountsParameterBase>(request, response);

				void setData()
				{
					response.LastPage = data.GetPageCount(request.PerPage) - 1;
					response.Data = data.Paginate(request.PerPage, request.Page).Select(a => a.Account.CloneAsT()).ToList();
				}

				await CachedCriterionChain.Cache.Get()
					.ThisValidatePagination(request, data)
					.SetOnCriterionMet(setData)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<RealAccountEntity>> SearchRealAccounts(IAsAccountGetPaginatedEncryptableRequest<IAccountService.SearchRealAccountsParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<RealAccountEntity>();

			async Task notNullRequest()
			{
				var data = await SearchAccounts<RealAccountEntity, RealAccountSettingsEntity, IAccountService.SearchRealAccountsParameter>(request, response);
				if (request.Parameter.CanBeDepositAccountFor?.Any() ?? false)
				{
					data = data.Where(t => request.Parameter.CanBeDepositAccountFor.All(kvp1 => t.Settings.CanBeDepositAccountFor.Any(kvp2 => kvp2.Key.Id == kvp1.Key && kvp2.Value == kvp1.Value)));
				}
				if (request.Parameter.OwnerId.HasValue)
				{
					var owner = Connection.GetSingle<IEntity>(request.Parameter.OwnerId.Value);

					void ownerNotNull()
					{
						data = data.Where(t => owner.HoldsOwnerRight(Connection, t.Account));
					}

					await FirstNullCheck(owner,
							ValidationField.Create(nameof(request.Parameter.OwnerId)),
							ValidationCode.NotFound.WithMessage("The owner provided could not be found."))
						.SetOnCriterionMet(ownerNotNull)
						.Evaluate(response);
				}
				void setData()
				{
					response.LastPage = data.GetPageCount(request.PerPage) - 1;
					response.Data = data.Paginate(request.PerPage, request.Page).Select(a => a.Account.CloneAsT()).ToList();
				}
				await CachedCriterionChain.Cache.Get()
					.ThisValidatePagination(request, data)
					.SetOnCriterionMet(setData)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<VirtualAccountEntity>> SearchVirtualAccounts(IAsAccountGetPaginatedEncryptableRequest<IAccountService.SearchVirtualAccountsParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<VirtualAccountEntity>();

			async Task notNullRequest()
			{
				var data = await SearchAccounts<VirtualAccountEntity, VirtualAccountSettingsEntity, IAccountService.SearchVirtualAccountsParameter>(request, response);

				if (request.Parameter.DepositForwardLifeSpan.HasValue)
				{
					data = data.Where(t => t.Settings.DepositForwardLifeSpan == request.Parameter.DepositForwardLifeSpan);
				}
				if (request.Parameter.AdminIds?.Any() ?? false)
				{
					data = data.Where(t =>
					{
						var admins = t.Account.GetAdminClaimsHolders<IEntity>(Connection).Select(a => a.Id);
						return request.Parameter.AdminIds.All(admins.Contains);
					});
				}

				void setData()
				{
					response.LastPage = data.GetPageCount(request.PerPage) - 1;
					response.Data = data.Paginate(request.PerPage, request.Page).Select(a => a.Account.CloneAsT()).ToList();
				}
				await CachedCriterionChain.Cache.Get()
					.ThisValidatePagination(request, data)
					.SetOnCriterionMet(setData)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public VirtualAccountSettingsEntity UpdateVirtualAccountSettings(VirtualAccountEntity account)
		{
			VirtualAccountSettingsEntity settings = GetSettings<VirtualAccountSettingsEntity>(account);

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
				if (settings.Is<VirtualAccountSettingsEntity>(out var virtualAccountSettings, Connection))
				{
					OnVirtualAccountSettingsChanged.Invoke(Session,
					   virtualAccountSettings,
					   virtualAccountSettings.CloneAsT());
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

		public void UpdateDepositBalance(VirtualAccountEntity account, RealAccountEntity mappedAccount, Decimal value)
		{
			UpdateDepositBalance(account.DepositReferences.Single(r => r.ReferencedAccount.Id == mappedAccount.Id), value);
		}
	}
}
