using CBApplication.Requests;
using CBApplication.Requests.Abstractions;
using CBApplication.Services.Abstractions;

using CBCommon.Extensions;

using CBData.Entities;

using PBApplication.Events;
using PBApplication.Extensions;
using PBApplication.Requests;
using PBApplication.Requests.Abstractions;
using PBApplication.Responses;
using PBApplication.Responses.Abstractions;
using PBApplication.Services.Abstractions;
using PBCommon.Validation;

using PBCommon;
using PBCommon.Extensions;

using PBData.Abstractions;
using PBData.Entities;
using PBData.Extensions;

using ScrapeX;

using System;
using System.Collections.Generic;
using System.Linq;

using static CBApplication.Services.Abstractions.ICitizenService;
using static PBCommon.Enums;
using System.Threading.Tasks;
using static CBApplication.Services.Abstractions.ICitizenServiceBase;
using PBApplication.Context.Abstractions;

namespace CBApplication.Services
{
	public class CitizenService : CBService, ICitizenService
	{
		public CitizenService(IServiceContext serviceContext) : base(serviceContext)
		{
			Observe<ICitizenService>(this);
		}

		public event ServiceEventHandler<ServiceEventArgs<CitizenLinkRequestEntity>> OnCitizenLinkRequestCreated;
		public async Task<IResponse> CreateCitizenLinkRequest(IAsUserRequest<CreateCitizenLinkRequestParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				CitizenEntity citizen = await GetCitizen(request.Parameter.Name);

				void successAction()
				{
					/*
					 * TODO: check if account has sc-package
					 * var record = "<span class=\"label\">UEE Citizen Record</span><strong class=\"value\">#2405299</strong>";
					 */
					var verification = GetService<IEventfulCUDService>().GenerateUniqueVerification();
					var createRequest = new CitizenLinkRequestEntity(user, citizen, verification);
					createRequest.RefreshNow();
					Connection.Insert(createRequest);
					Connection.SaveChanges();

					OnCitizenLinkRequestCreated.Invoke(Session,
										user,
										createRequest.CloneAsT());
				}

				await FirstValidateAsUser(user, response.Validation)
					.NextNullCheck(citizen,
						response.Validation.GetField(nameof(request.Parameter.Name)),
						DefaultCode.NotFound.SetMessage("The requested citizen could not be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public event ServiceEventHandler<ServiceEventArgs> OnCitizenLinkRequestCancelled;
		public async Task<IResponse> CancelCitizenLinkRequest(IAsUserEncryptableRequest<CancelCitizenLinkRequestParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenLinkRequestEntity> createRequest = Connection.GetSingleLazily<CitizenLinkRequestEntity>(request.Parameter.RequestId);

				Boolean userOwnsRequestCheck()
				{
					return user.Id == createRequest.Value.Owner.Id;
				}
				void successAction()
				{
					Connection.Delete(createRequest.Value);
					Connection.SaveChanges();

					OnCitizenLinkRequestCancelled.Invoke(createRequest.Value);

					LogIfAccessingAsDelegate(user, "cancelled citizen link request for " + createRequest.Value.CitizenName);
				}

				await FirstValidateAsUser(user, response.Validation)
					.NextCompound(userOwnsRequestCheck,
						response.Validation.GetField(nameof(request.Parameter.RequestId)),
						DefaultCode.NotFound.SetMessage("The request requested could not be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public event ServiceEventHandler<ServiceEventArgs> OnCitizenLinkRequestVerified;
		public event ServiceEventHandler<ServiceEventArgs<CitizenEntity>> OnCitizenLinked;

		public async Task<IResponse> VerifyCitizenLinkRequest(IAsUserEncryptableRequest<VerifyCitizenLinkRequestParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenLinkRequestEntity> createRequest = Connection.GetSingleLazily<CitizenLinkRequestEntity>(request.Parameter.RequestId);

				Boolean userOwnsRequestCheck()
				{
					return user.Id == createRequest.Value.Owner.Id;
				}
				Boolean validCheck()
				{
					String url = "https://robertsspaceindustries.com/citizens/" + createRequest.Value.CitizenName;

					var scraperFactory = new ScraperFactory();

					Boolean retVal = false;
					String bio = "bio";

					scraperFactory.CreateSinglePageScraper(url)
						.SetTargetPageXPaths(new Dictionary<String, String>
						{
						{bio,     "/html/body/div[2]/div[2]/div[2]/div/div/div[2]/div[3]/div/div/div" }
						}).Go((link, dict) => retVal = dict[bio]?.Contains(createRequest.Value.VerificationCode.ToVerifyLink()) ?? false);

#if DEBUG
					//TODO: remove if not needed anymore
					retVal = true;
#endif

					return retVal;
				}
				async Task successAction()
				{
					var citizen = await GetCitizen(createRequest.Value.CitizenName);
					citizen.Owner = createRequest.Value.Owner;
					Connection.Update(citizen);
					Connection.Delete(createRequest.Value);
					Connection.SaveChanges();

					OnCitizenLinked.Invoke(Session, new IEntity[] { createRequest.Value.Owner, citizen }, citizen.CloneAsT());
					OnCitizenLinkRequestVerified.Invoke(createRequest.Value);

					GetService<IEventfulUserRoleService>().TryAddToRole(Session.Owner,
																	Connection.GetFirst<UserRoleEntity>(r => r.Name.Equals(CBCommon.Settings.CitizenBank.CITIZEN_ROLE)),
																	Connection.Query<UserRoleEntity>().Where(r => r.Name.Equals(PBCommon.Settings.SYSTEM_ROLE)));

					LogIfAccessingAsDelegate(user, "verified citizen link request for " + citizen.Name);
				}

				await FirstValidateAsUser(user, response.Validation)
					.NextNullCheck(createRequest,
						response.Validation.GetField(nameof(request.Parameter.RequestId)),
						DefaultCode.NotFound.SetMessage("The request requested could not be found."))
					.NextCompound(userOwnsRequestCheck,
						DefaultCode.Unauthorized.SetMessage("You are not authorized to request verification for this request."))
					.NextCompound(validCheck,
						DefaultCode.Invalid.SetMessage("The request could not be verified."))
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task EnsureSettingsAndAccountForCitizen(CitizenEntity citizen)
		{
			void run()
			{
				if (GetSettings<CitizenSettingsEntity, CitizenEntity>(citizen) == null)
				{
					//TODO: set default accessibility to private
					var citizenSettings = new CitizenSettingsEntity(citizen)
					{
						Accessibility = AccessibilityType.Public
					};
					Connection.Insert(citizenSettings);
				}
				if (Connection.GetSingle<RealAccountEntity>(a => a.Owner.Id == citizen.Id) == null)
				{
					List<CurrencyEntity> currencies = Connection.Query<CurrencyEntity>().Where(c => c.IsActive).ToList();

					CreditScoreEntity creditScore = new CreditScoreEntity();
					RealAccountEntity account = new RealAccountEntity(citizen, creditScore);
					//TODO: set default accessibility to private
					RealAccountSettingsEntity accountSettings = new RealAccountSettingsEntity(account,
													  new CurrencyBoolDictionaryEntity(currencies, true),
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
				}
				Connection.SaveChanges();
			}
			await Task.Run(run);
		}

		public async Task<IGetPaginatedEncryptableResponse<CitizenLinkRequestEntity>> GetCitizenLinkRequests(IGetPaginatedAsUserRequest<GetCitizenLinkRequestsParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<CitizenLinkRequestEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);

				async Task successAction()
				{
					GetService<IEventfulManageExpirantsService>().DeleteExpirants<CitizenLinkRequestEntity>();

					var query = Connection.Query<CitizenLinkRequestEntity>()
						.Where(r => r.Owner.Id == user.Id);

					void setData()
					{
						response.Data = query
						.Paginate(request.PerPage, request.Page)
						.Select(r => r.CloneAsT())
						.ToList();
						response.LastPage = query.GetPageCount(request.PerPage) - 1;

						LogIfAccessingAsDelegate(user, "retrieved citizen link requests for :\n" + String.Join("\n", response.Data.Select(r => r.CitizenName)));
					}

					await CachedCriterionChain.Cache.Get()
						.ThisValidatePagination(request, query, response.Validation)
						.SetOnCriterionMet(setData)
						.Evaluate();
				}

				await FirstValidateAsUser(user, response.Validation)
					.SetOnCriterionMet(successAction)
					.Evaluate();

			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}
		public async Task<IGetPaginatedEncryptableResponse<CitizenLinkRequestEntity>> GetCitizenLinkRequests()
		{
			var response = new GetPaginatedEncryptableResponse<CitizenLinkRequestEntity>();

			void successAction()
			{
				response.Data = Connection.Query<CitizenLinkRequestEntity>()
					.Where(r => r.Owner.Id == Session.Owner.Id)
					.Select(r => r.CloneAsT())
					.ToList();
			}

			await FirstValidateAuthenticated(response.Validation)
				.SetOnCriterionMet(successAction)
				.Evaluate();

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<CitizenEntity>> GetCitizens(IGetPaginatedAsUserRequest<GetCitizensParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<CitizenEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);

				async Task successAction()
				{
					var query = Connection.Query<CitizenEntity>()
						.Where(c => c.Owner.Id == user.Id);

					void setData()
					{
						response.Data = query
						.Paginate(request.PerPage, request.Page)
						.Select(c => c.CloneAsT())
						.ToList();

						LogIfAccessingAsDelegate(user, "retrieved citizens :\n" + String.Join("\n", response.Data.Select(c => c.Name)));
					}

					await CachedCriterionChain.Cache.Get()
						.ThisValidatePagination(request, query, response.Validation)
						.SetOnCriterionMet(setData)
						.Evaluate();
				}

				await FirstValidateAsUser(user, response.Validation)
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}
		public async Task<IGetPaginatedEncryptableResponse<CitizenEntity>> GetCitizens()
		{
			var response = new GetPaginatedEncryptableResponse<CitizenEntity>();

			void successAction()
			{
				response.Data = Connection.Query<CitizenEntity>()
					.Where(c => c.Owner.Id == Session.Owner.Id)
					.Select(c => c.CloneAsT())
					.ToList();
			}

			await FirstValidateAuthenticated(response.Validation)
				.SetOnCriterionMet(successAction)
				.Evaluate();

			return response;
		}

		public async Task<IEncryptableResponse<CitizenSettingsEntity>> GetCitizenSettings(IAsCitizenRequest request)
		{
			var response = new EncryptableResponse<CitizenSettingsEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);

				void successAction()
				{
					response.Overwrite(Connection.GetSingle<CitizenSettingsEntity>(s => s.Owner.Id == citizen.Value.Id).CloneAsT());
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

		public event ServiceEventHandler<ServiceEventArgs<CitizenSettingsEntity>> OnCitizenSettingsChanged;
		public async Task<IResponse> SetCitizenSettings(IAsCitizenRequest<SetCitizenSettingsParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<CitizenSettingsEntity> settings = Connection.GetFirstLazily<CitizenSettingsEntity>(s => s.Owner.Id == citizen.Value.Id);

				void successAction()
				{
					if (request.Parameter.CanBeRecruitedAsDepartmentAdmin.HasValue)
					{
						settings.Value.CanBeRecruitedAsDepartmentAdmin = request.Parameter.CanBeRecruitedAsDepartmentAdmin.Value;
					}
					if (request.Parameter.Accessibility.HasValue)
					{
						settings.Value.Accessibility = request.Parameter.Accessibility.Value;
					}
					if (settings.IsValueCreated)
					{
						Connection.Update(settings);
						Connection.SaveChanges();

						OnCitizenSettingsChanged.Invoke(Session, settings.Value, settings.Value.CloneAsT());
					}
				}

				await FirstValidateAsCitizen(user, citizen, response.Validation)
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public event ServiceEventHandler<ServiceEventArgs<ICitizenService.OnCitizenUnlinkedData>> OnCitizenUnlinked;
		public async Task<IResponse> UnlinkCitizen(IAsCitizenRequest request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);

				void successAction()
				{
					var previousOwner = citizen.Value.Owner;

					citizen.Value.Owner = null;
					Connection.Update(citizen);
					Connection.SaveChanges();

					OnCitizenUnlinked.Invoke(Session, citizen.Value, new ICitizenService.OnCitizenUnlinkedData(citizen.Value.CloneAsT(), previousOwner.CloneAsT()));

					if (!Connection.Query<CitizenEntity>().Where(c => c.Owner.Id == user.Id).Any())
					{
						GetService<IEventfulUserRoleService>().TryRemoveFromRole(user,
																		Connection.GetSingle<UserRoleEntity>(r => r.Name.Equals(CBCommon.Settings.CitizenBank.CITIZEN_ROLE)),
																		Connection.Query<UserRoleEntity>().Where(r => r.Name.Equals(PBCommon.Settings.SYSTEM_ROLE)));
					}

					LogIfAccessingAsDelegate(user, "unlinked citizen " + citizen.Value.Name);
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

		public async Task<IGetPaginatedEncryptableResponse<CitizenEntity>> SearchCitizens(IGetPaginatedAsUserEncryptableRequest<SearchCitizensParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<CitizenEntity>();

			async Task notNullRequest()
			{
				Lazy<IQueryable<CitizenEntity>> query = new Lazy<IQueryable<CitizenEntity>>(() =>
				{
					var retVal1 = Connection.Query<CitizenSettingsEntity>();
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

						FirstValidateAsUser(user, response.Validation)
							.NextCompound(userIsInRoleCheck)
							.SetOnCriterionMet(accessibilitySuccessAction)
							.Evaluate();
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
					IQueryable<CitizenEntity> retVal2 = retVal1.Select(s => s.Owner);
					if (request.Parameter.ExcludeIds?.Any() ?? false)
					{
						retVal2 = retVal2.Where(a => !request.Parameter.ExcludeIds.Contains(a.Id));
					}
					if (request.Parameter.ExcludeNames?.Any() ?? false)
					{
						retVal2 = retVal2.Where(a => !request.Parameter.ExcludeNames.Contains(a.Name.ToLower()));
					}
					if (request.Parameter.Name != null)
					{
						retVal2 = retVal2.Where(s => s.Name.Equals(request.Parameter.Name) || s.Name.ToLower().Contains(request.Parameter.Name.ToLower()));
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
					if (response.Data.Count() == 0 && newCitizen != null)
					{
						response.Data = new List<CitizenEntity>
						{
							newCitizen.CloneAsT()
						};
						response.LastPage = 0;
					}
				}

				await CachedCriterionChain.Cache.Get()
					.ThisValidatePagination(request, query.Value, response.Validation)
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<CitizenEntity> GetCitizen(String name)
		{
			CitizenEntity citizen = null;
			if (!name.IsValidHandle())
			{
				return null;
			}
			name = name.ToLower();
			if ((citizen = Connection.GetSingle<CitizenEntity>(c => c.Name.ToLower().Equals(name))) == null)
			{
				String url = "https://robertsspaceindustries.com/citizens/" + name;
				String actualName = String.Empty;
				ScraperFactory scraperFactory = new ScraperFactory();
				scraperFactory.CreateSinglePageScraper(url)
					.SetTargetPageXPaths(new Dictionary<String, String>
					{
							{nameof(actualName),     "/html/body/div[2]/div[2]/div[2]/div/div/div[2]/div[1]/div/div[1]/div/div[2]/p[2]/strong" }
					}).Go((String link, IDictionary<String, String> dict) => actualName = dict[nameof(actualName)]);
				if (actualName == null)
				{
					return null;
				}
				Connection.Insert(citizen = new CitizenEntity(actualName) );
				await EnsureSettingsAndAccountForCitizen(citizen);
				Connection.SaveChanges();
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
					response.Validation.GetField(nameof(name)),
					DefaultCode.NotFound.SetMessage("The requested citizen could not be found."))
				.SetOnCriterionMet(successAction)
				.Evaluate();

			return response;
		}
	}
}
