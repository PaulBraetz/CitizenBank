using CBApplication.Requests.Abstractions;
using CBApplication.Services.Abstractions;

using CBCommon.Extensions;

using CBData.Entities;

using static CBApplication.Services.Abstractions.ICitizenService;

namespace CBApplication.Services
{
    public class CitizenService : CBService, IEventfulCitizenService
	{
		public CitizenService(IServiceContext serviceContext) : base(serviceContext)
		{
			Observe<IEventfulCitizenService>(this);
		}

		public event ServiceEventHandler<ServiceEventArgs<CitizenLinkRequestEntity>> OnCitizenLinkRequestCreated;
		public async Task<IResponse> CreateCitizenLinkRequest(IAsUserRequest<CreateCitizenLinkRequestParameter> request)
		{

			var response = new Response();

			async Task notNullRequest()
			{
				CitizenEntity citizen = await GetCitizen(request.Parameter.Name);

				void successAction()
				{
					/*
					 * TODO: check if account has sc-package
					 * var record = "<span class=\"label\">UEE Citizen Record</span><strong class=\"value\">#2405299</strong>";
					 */
					UserEntity user = GetUserEntity(request);

					var verification = GetService<IEventfulCUDService>().GenerateUniqueVerification();
					var createRequest = new CitizenLinkRequestEntity(user, citizen, verification);
					createRequest.RefreshNow();
					Connection.Insert(createRequest);
					Connection.SaveChanges();

					OnCitizenLinkRequestCreated.Invoke(Session,
										user,
										createRequest.CloneAsT());
				}

				await FirstValidateAuthenticatedDelegate(request, response)
					.NextNullCheck(citizen,
						ValidationField.Create(nameof(request.Parameter.Name)),
						ValidationCode.NotFound.WithMessage("The requested citizen could not be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}

		public event ServiceEventHandler<ServiceEventArgs> OnCitizenLinkRequestCancelled;
		public async Task<IResponse> CancelCitizenLinkRequest(IAsUserEncryptableRequest<CancelCitizenLinkRequestParameter> request)
		{

			var response = new Response();

			async Task notNullRequest()
			{
				var user = GetUserEntity(request);
				var createRequest = Connection.GetSingle<CitizenLinkRequestEntity>(request.Parameter.RequestId);

				Boolean userOwnsRequestCheck()
				{
					return user.Id == createRequest.User.Id;
				}
				void successAction()
				{
					Connection.Delete(createRequest);
					Connection.SaveChanges();

					OnCitizenLinkRequestCancelled.Invoke(createRequest);

					LogIfAccessingAsDelegate(user, "cancelled citizen link request for {0}", createRequest.Citizen.Name);
				}

				await FirstValidateAuthenticatedDelegate(request, response)
					.NextNullCheck(createRequest,
						ValidationField.Create(nameof(request.Parameter.RequestId)),
						ValidationCode.NotFound.WithMessage("The link request requested could not be found."))
					.NextCompound(userOwnsRequestCheck,
						ValidationCode.Unauthorized.WithMessage("You are not authorized to cancel this link request."))
					.InheritField()
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}

		public event ServiceEventHandler<ServiceEventArgs> OnCitizenLinkRequestVerified;
		public event ServiceEventHandler<ServiceEventArgs<CitizenEntity>> OnCitizenLinked;

		public async Task<IResponse> VerifyCitizenLinkRequest(IAsUserEncryptableRequest<VerifyCitizenLinkRequestParameter> request)
		{

			var response = new Response();

			async Task notNullRequest()
			{
				var user = GetUserEntity(request);
				var createRequest = Connection.GetSingle<CitizenLinkRequestEntity>(request.Parameter.RequestId);

				Boolean userOwnsRequestCheck()
				{
					return user.Id == createRequest.User.Id;
				}
				Boolean validCheck()
				{
					String url = "https://robertsspaceindustries.com/citizens/" + createRequest.Citizen.Name;

					var scraperFactory = new ScraperFactory();

					Boolean retVal = false;
					String bio = "bio";

					scraperFactory.CreateSinglePageScraper(url)
						.SetTargetPageXPaths(new Dictionary<String, String>
						{
						{bio,     "/html/body/div[2]/div[2]/div[2]/div/div/div[2]/div[3]/div/div/div" }
						}).Go((link, dict) => retVal = dict[bio]?.Contains(createRequest.VerificationCode.ToVerifyLink()) ?? false);

#if DEBUG
					//TODO: remove if not needed anymore
					retVal = true;
#endif

					return retVal;
				}
				void successAction()
				{
					var citizen = createRequest.Citizen;
					var previousOwners = citizen.GetOwnerClaimsHolders<UserEntity>(Connection);

					var claimService = GetService<IEventfulClaimService>();

					var previousOwnersDatum = new IClaimService.EnsureRightDatum(PBCommon.Configuration.Settings.OwnerRight, false);
					previousOwners.ForEach(c => claimService.EnsureClaim(c, citizen, previousOwnersDatum));
					claimService.EnsureClaim(user, citizen, new IClaimService.EnsureRightDatum(PBCommon.Configuration.Settings.OwnerRight, true));
					claimService.EnsureClaim(user, new IClaimService.EnsureRightDatum(CBCommon.Settings.CitizenBank.CITIZEN_RIGHT, true));

					Connection.Delete(createRequest);
					Connection.SaveChanges();

					OnCitizenLinked.Invoke(Session, new IEntity[] { createRequest.User, citizen }, citizen.CloneAsT());
					OnCitizenLinkRequestVerified.Invoke(createRequest);

					LogIfAccessingAsDelegate(user, "verified citizen link request for {0}", citizen.Name);
				}

				await FirstValidateAuthenticatedDelegate(request, response)
					.NextNullCheck(createRequest,
						ValidationField.Create(nameof(request.Parameter.RequestId)),
						ValidationCode.NotFound.WithMessage("The link request requested could not be found."))
					.NextCompound(userOwnsRequestCheck,
						ValidationCode.Unauthorized.WithMessage("You are not authorized to request verification for this request."))
					.InheritField()
					.NextCompound(validCheck,
						ValidationCode.Invalid.WithMessage("The request could not be verified."))
					.InheritField()
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}

		public async Task EnsureSettingsAndAccountForCitizen(CitizenEntity citizen)
		{

			void run()
			{
				var claimService = GetService<IEventfulClaimService>();
				if (GetSettings<CitizenSettingsEntity>(citizen) == null)
				{
					//TODO: set default accessibility to private
					var citizenSettings = new CitizenSettingsEntity()
					{
						Accessibility = AccessibilityType.Public
					};
					Connection.Insert(citizenSettings);

					//TODO: Settings ownership is immutable => use property instead
					claimService.EnsureClaim(citizen, citizenSettings, new IClaimService.EnsureRightDatum(PBCommon.Configuration.Settings.OwnerRight, true));
				}
				if (!citizen.GetHeldOwnerClaimsValues<RealAccountEntity>(Connection).Any())
				{
					List<CurrencyEntity> currencies = Connection.Query<CurrencyEntity>().Where(c => c.IsActive).ToList();

					CreditScoreEntity creditScore = new();
					RealAccountEntity account = new(citizen, creditScore);
					//TODO: set default accessibility to private
					RealAccountSettingsEntity accountSettings = new(new CurrencyBoolDictionaryEntity(currencies, true),
													  new CurrencyBoolDictionaryEntity(currencies, true),
													  new CurrencyBoolDictionaryEntity(currencies),
													  new CurrencyBoolDictionaryEntity(currencies))
					{
						Accessibility = AccessibilityType.Public
					};

					Connection.Insert(creditScore,
									account,
									accountSettings.CanBeDepositAccountFor,
									accountSettings.CanBeMiddlemanFor,
									accountSettings.CanCreateTransactionOffersFor,
									accountSettings.CanReceiveTransactionOffersFor,
									accountSettings);

					claimService.EnsureClaim(account, accountSettings, new IClaimService.EnsureRightDatum(PBCommon.Configuration.Settings.OwnerRight, true));
					claimService.EnsureClaim(citizen, account, new IClaimService.EnsureRightDatum(PBCommon.Configuration.Settings.OwnerRight, true));
				}
				Connection.SaveChanges();
			}
			await Task.Run(run);
		}

		public async Task<IGetPaginatedEncryptableResponse<CitizenLinkRequestEntity>> GetCitizenLinkRequests(IAsUserGetPaginatedRequest<GetCitizenLinkRequestsParameter> request)
		{

			var response = new GetPaginatedEncryptableResponse<CitizenLinkRequestEntity>();

			async Task notNullRequest()
			{
				async Task successAction()
				{
					UserEntity user = GetUserEntity(request);

					GetService<IEventfulManageExpirantsService>().DeleteExpirants<CitizenLinkRequestEntity>();

					var query = Connection.Query<CitizenLinkRequestEntity>()
						.Where(r => r.User.Id == user.Id);

					void setData()
					{
						response.Data = query
						.Paginate(request.PerPage, request.Page)
						.Select(r => r.CloneAsT())
						.ToList();
						response.LastPage = query.GetPageCount(request.PerPage) - 1;

						LogIfAccessingAsDelegate(user, "retrieved citizen link requests for : {0}", String.Join(", ", response.Data.Select(r => r.Citizen.Name)));
					}

					await CachedCriterionChain.Cache.Get()
						.ThisValidatePagination(request, query)
						.SetOnCriterionMet(setData)
						.Evaluate(response);
				}

				await FirstValidateAuthenticatedDelegate(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);

			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}
		public async Task<IGetPaginatedEncryptableResponse<CitizenLinkRequestEntity>> GetCitizenLinkRequests()
		{

			var response = new GetPaginatedEncryptableResponse<CitizenLinkRequestEntity>();

			void successAction()
			{
				response.Data = Connection.Query<CitizenLinkRequestEntity>()
					.Where(r => r.User.Id == Session.User.Id)
					.CloneAsT()
					.ToList();
			}

			await FirstValidateAuthenticated()
				.SetOnCriterionMet(successAction)
				.CatchAll(ValidationField.Create("request"))
				.Evaluate(response);

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<CitizenEntity>> GetCitizens(IAsUserGetPaginatedRequest<GetCitizensParameter> request)
		{

			var response = new GetPaginatedEncryptableResponse<CitizenEntity>();

			async Task notNullRequest()
			{
				async Task successAction()
				{
					UserEntity user = GetUserEntity(request);

					var query = user.GetHeldOwnerClaimsValuesRecursively<CitizenEntity>(Connection);

					void setData()
					{
						response.Data = query
						.Paginate(request.PerPage, request.Page)
						.CloneAsT()
						.ToList();

						LogIfAccessingAsDelegate(user, "retrieved citizens :{0}", String.Join(", ", response.Data.Select(c => c.Name)));
					}

					await CachedCriterionChain.Cache.Get()
						.ThisValidatePagination(request, query)
						.SetOnCriterionMet(setData)
						.Evaluate(response);
				}

				await FirstValidateAuthenticatedDelegate(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}
		public async Task<IGetPaginatedEncryptableResponse<CitizenEntity>> GetCitizens()
		{
			var response = new GetPaginatedEncryptableResponse<CitizenEntity>();

			void successAction()
			{
				response.Data = Session.User.GetHeldOwnerClaimsValuesRecursively<CitizenEntity>(Connection)
					.CloneAsT()
					.ToList();
			}

			await FirstValidateAuthenticated()
				.SetOnCriterionMet(successAction)
				.CatchAll(ValidationField.Create("request"))
				.Evaluate(response);

			return response;
		}

		public async Task<IEncryptableResponse<CitizenSettingsEntity>> GetCitizenSettings(IAsCitizenRequest request)
		{
			var response = new EncryptableResponse<CitizenSettingsEntity>();

			async Task notNullRequest()
			{
				void successAction()
				{
					var citizen = GetCitizenEntity(request);
					var settings = GetSettings<CitizenSettingsEntity>(citizen);

					response.Overwrite(settings);

					LogIfAccessingAsDelegate(GetUserEntity(request), "retrieved citizen settings for :{0}", citizen.Name);
				}

				await FirstValidateAsCitizen(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}

		public event ServiceEventHandler<ServiceEventArgs<CitizenSettingsEntity>> OnCitizenSettingsChanged;
		public async Task<IResponse> SetCitizenSettings(IAsCitizenRequest<SetCitizenSettingsParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				void successAction()
				{
					var citizen = GetCitizenEntity(request);
					var settings = GetSettings<CitizenSettingsEntity>(citizen);
					Boolean changed = false;

					if (request.Parameter.CanBeRecruitedAsDepartmentAdmin.HasValue)
					{
						settings.CanBeRecruitedAsDepartmentAdmin = request.Parameter.CanBeRecruitedAsDepartmentAdmin.Value;
						changed = true;
					}
					if (request.Parameter.Accessibility.HasValue)
					{
						settings.Accessibility = request.Parameter.Accessibility.Value;
						changed = true;
					}
					if (changed)
					{
						Connection.Update(settings);
						Connection.SaveChanges();

						OnCitizenSettingsChanged.Invoke(Session, settings, settings.CloneAsT());

						LogIfAccessingAsDelegate(GetUserEntity(request), "set citizen settings for :{0}", citizen.Name);
					}
				}

				await FirstValidateAsCitizen(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}

		public event ServiceEventHandler<ServiceEventArgs<IEventfulCitizenService.OnCitizenUnlinkedData>> OnCitizenUnlinked;
		public async Task<IResponse> UnlinkCitizen(IAsCitizenRequest request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				void successAction()
				{
					var citizen = GetCitizenEntity(request);

					var claimService = GetService<IEventfulClaimService>();

					var previousOwner = citizen.GetOwnerClaimsHolders<UserEntity>(Connection).Single();

					claimService.EnsureClaim(previousOwner, citizen, new IClaimService.EnsureRightDatum(PBCommon.Configuration.Settings.OwnerRight, false));

					if (!previousOwner.GetHeldOwnerClaimsValues<CitizenEntity>(Connection).Any())
					{
						claimService.EnsureClaim(previousOwner, new IClaimService.EnsureRightDatum(CBCommon.Settings.CitizenBank.CITIZEN_RIGHT, false));
					}

					OnCitizenUnlinked.Invoke(Session, citizen, new IEventfulCitizenService.OnCitizenUnlinkedData(citizen.CloneAsT(), previousOwner.CloneAsT()));

					LogIfAccessingAsDelegate(GetUserEntity(request), "unlinked citizen :{0}", citizen.Name);
				}

				await FirstValidateAsCitizen(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<CitizenEntity>> SearchCitizens(IAsUserGetPaginatedEncryptableRequest<SearchCitizensParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<CitizenEntity>();

			async Task notNullRequest()
			{
				Lazy<IEnumerable<CitizenEntity>> query = new(() =>
				{
					var retVal1 = Connection.Query<CitizenSettingsEntity>();
					if (request.Parameter.Accessibility.HasValue && request.Parameter.Accessibility.Value == AccessibilityType.Private)
					{
						void accessibilitySuccessAction()
						{
							retVal1 = retVal1.Where(s => s.Accessibility == AccessibilityType.Private);
						}

						FirstValidateAuthenticatedDelegate(request, response)
							.SetOnCriterionMet(accessibilitySuccessAction)
							.Evaluate(response);
					}
					else
					{
						retVal1 = retVal1.Where(s => s.Accessibility == AccessibilityType.Public);
					}
					if (request.Parameter.CanBeRecruitedAsDepartmentAdmin.HasValue)
					{
						retVal1 = retVal1.Where(s => s.CanBeRecruitedAsDepartmentAdmin == request.Parameter.CanBeRecruitedAsDepartmentAdmin.Value);
					}
					if (request.Parameter.CanBeRecruitedAsAccountAdmin.HasValue)
					{
						retVal1 = retVal1.Where(s => s.CanBeRecruitedAsAccountAdmin == request.Parameter.CanBeRecruitedAsAccountAdmin.Value);
					}

					var retVal2 = retVal1.ToArray().SelectMany(s => s.GetOwnerClaimsHolders<CitizenEntity>(Connection));
					if (request.Parameter.ExcludeIds?.Any() ?? false)
					{
						retVal2 = retVal2.Where(c => !request.Parameter.ExcludeIds.Contains(c.Id));
					}
					if (request.Parameter.ExcludeNames?.Any() ?? false)
					{
						retVal2 = retVal2.Where(c => !request.Parameter.ExcludeNames.Contains(c.Name.ToLower()));
					}
					if (request.Parameter.Name != null)
					{
						retVal2 = retVal2.Where(c => c.Name.Equals(request.Parameter.Name) || c.Name.ToLower().Contains(request.Parameter.Name.ToLower()));
					}
					return retVal2;
				});

				async Task successAction()
				{
					response.Data = query.Value
						.Paginate(request.PerPage, request.Page)
						.CloneAsT()
						.ToList();

					response.LastPage = query.Value.GetPageCount(request.PerPage) - 1;

					CitizenEntity newCitizen = await GetCitizen(request.Parameter.Name);
					if (response.Data.Count == 0 && newCitizen != null)
					{
						response.Data = new List<CitizenEntity>
						{
							newCitizen.CloneAsT()
						};
						response.LastPage = 0;
					}
				}

				await CachedCriterionChain.Cache.Get()
					.ThisValidatePagination(request, query.Value)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}

		public async Task<CitizenEntity> GetCitizen(String name)
		{
			CitizenEntity citizen = null;
			if (name.IsValidHandle())
			{
				name = name.ToLower();
				citizen = Connection.GetSingle<CitizenEntity>(c => c.Name.ToLower().Equals(name));
				if (citizen == null)
				{
					String url = "https://robertsspaceindustries.com/citizens/" + name;
					String actualName = String.Empty;
					ScraperFactory scraperFactory = new();
					scraperFactory.CreateSinglePageScraper(url)
						.SetTargetPageXPaths(new Dictionary<String, String>
						{
							{nameof(actualName),     "/html/body/div[2]/div[2]/div[2]/div/div/div[2]/div[1]/div/div[1]/div/div[2]/p[2]/strong" }
						}).Go((String link, IDictionary<String, String> dict) => actualName = dict[nameof(actualName)]);
					if (actualName == null)
					{
						return null;
					}
					Connection.Insert(citizen = new CitizenEntity(actualName));
					await EnsureSettingsAndAccountForCitizen(citizen);
					Connection.SaveChanges();
				}
			}
			return citizen;
		}

		public async Task<IEncryptableResponse<CitizenEntity>> RetrieveCitizen(String name)
		{
			var response = new EncryptableResponse<CitizenEntity>();

			var citizen = await GetCitizen(name);

			void successAction()
			{
				response.Overwrite(citizen);
			}

			await FirstNullCheck(citizen,
					ValidationField.Create(nameof(name)),
					ValidationCode.NotFound.WithMessage("The requested citizen could not be found."))
				.SetOnCriterionMet(successAction)
				.CatchAll(ValidationField.Create("request"))
				.Evaluate(response);

			return response;
		}

		public async Task<IResponse> SetCurrentCitizen(IEncryptableRequest<SetCurrentCitizenRequestParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				var citizen = Connection.GetSingle<CitizenEntity>(request.Parameter.CitizenId);

				void successAction()
				{
					var realAccount = citizen.GetHeldOwnerClaimsValues<RealAccountEntity>(Connection).Single();
					GetService<IEventfulUserService>().AttachToSession(realAccount);
					GetService<IEventfulUserService>().AttachToSession(citizen);
				}

				await FirstValidateAuthenticated()
					.NextNullCheck(citizen,
						ValidationField.Create(nameof(request.Parameter.CitizenId)),
						ValidationCode.NotFound)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Request)
				.Evaluate(response);

			return response;
		}

		public async Task<IEncryptableResponse<CitizenEntity>> GetCurrentCitizen()
		{
			var response = new EncryptableResponse<CitizenEntity>();


			void successAction()
			{
				response.Data = Session.GetAttached<CitizenEntity>().SingleOrDefault();
			}

			await FirstValidateAuthenticated()
				.SetOnCriterionMet(successAction)
				.Evaluate(response);

			return response;
		}
	}
}
