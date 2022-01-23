using CBApplication.Extensions;
using CBApplication.Requests;
using CBApplication.Requests.Abstractions;
using CBApplication.Services.Abstractions;

using CBCommon.Extensions;

using CBData.Abstractions;
using CBData.Entities;

using PBApplication.Events;
using PBApplication.Extensions;
using PBApplication.Requests.Abstractions;
using PBApplication.Responses;
using PBApplication.Responses.Abstractions;
using PBApplication.Services.Abstractions;
using PBCommon.Validation;
using PBCommon.Validation.Abstractions;

using PBCommon;
using PBCommon.Extensions;

using PBData.Entities;
using PBData.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

using static CBApplication.Services.Abstractions.IEventfulTransactionService;
using static CBCommon.Enums.CitizenBankEnums;
using System.Threading.Tasks;
using static CBApplication.Services.Abstractions.ITransactionService;
using PBApplication.Context.Abstractions;

namespace CBApplication.Services
{
	public class TransactionService : CBService, IEventfulTransactionService
	{
		public static readonly SimpleTransactionContractProperty DefaultTransactionContractFilterProperty = SimpleTransactionContractProperty.Created;

		public TransactionService(IServiceContext serviceContext) : base(serviceContext)
		{
			Observe<IEventfulTransactionService>(this);
		}

		private async Task<SourceTransactionContractEntity> CreateSourceTransactionContract(IAsAccountEncryptableRequest<CreateTransactionOfferParameter> request, IValidationFieldCollection fields)
		{
			UserEntity user = GetUserEntity(request);
			Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
			Lazy<AccountEntityBase> account = GetAccountEntityLazily<AccountEntityBase>(request);
			Lazy<AccountSettingsEntityBase> accountSettings = new(() => GetSettings<AccountSettingsEntityBase, AccountEntityBase>(account.Value));
			Lazy<CurrencyEntity> currency = Connection.GetSingleLazily<CurrencyEntity>(request.Parameter.CurrencyId);
			Lazy<AccountEntityBase> recipient = Connection.GetSingleLazily<AccountEntityBase>(request.Parameter.RecipientId);
			Lazy<AccountSettingsEntityBase> recipientSettings = new(() => GetSettings<AccountSettingsEntityBase, AccountEntityBase>(recipient.Value));
			Lazy<AccountEntityBase> creditor = Connection.GetSingleLazily<AccountEntityBase>(request.Parameter.CreditorId);
			Lazy<AccountEntityBase> debtor = Connection.GetSingleLazily<AccountEntityBase>(request.Parameter.DebtorId);

			SourceTransactionContractEntity retVal = null;

			Boolean creditorDebtorCheck()
			{
				return (creditor.Value.Id == account.Value.Id && debtor.Value.Id == recipient.Value.Id) || (debtor.Value.Id == account.Value.Id && creditor.Value.Id == recipient.Value.Id);
			}
			Boolean valueCheck()
			{
				return request.Parameter.Value > 1 && ((request.Parameter.Value - Math.Truncate(request.Parameter.Value)) == 0);
			}
			Boolean additionalDaysUntilDueCheck()
			{
				return request.Parameter.AdditionalDaysUntilDue <= (Int32)DefaultTimeSpanDays.Long && request.Parameter.AdditionalDaysUntilDue >= 0;
			}
			Boolean canReceiveCheck()
			{
				return recipientSettings.Value.CanReceiveTransactionOffersFor[currency.Value];
			}
			Boolean canCreateCheck()
			{
				return accountSettings.Value.CanCreateTransactionOffersFor[currency.Value];
			}
			async Task successAction()
			{
				TagEntity[] tags = request.Parameter.TagsTexts != null ? await Task.WhenAll(request.Parameter.TagsTexts.Select(t => GetService<IEventfulTagService>().GetTag(t))) : Array.Empty<TagEntity>();
				retVal = CreateSourceTransactionContract(account.Value,
					recipient.Value,
					creditor.Value,
					debtor.Value,
					request.Parameter.Value,
					currency.Value,
					request.Parameter.Usage,
					tags,
					TimeSpan.FromDays(request.Parameter.AdditionalDaysUntilDue));
			}

			await FirstValidateAsAccount(user, citizen, account, fields)
				.NextNullCheck(creditor, fields.GetField(nameof(request.Parameter.CreditorId)), DefaultCode.NotFound)
				.NextNullCheck(debtor, fields.GetField(nameof(request.Parameter.DebtorId)))
				.NextNullCheck(recipient, fields.GetField(nameof(request.Parameter.RecipientId)))
				.NextCompound(creditorDebtorCheck, fields.GetField(DefaultField.MiscellaneousName), DefaultCode.Invalid)
				.NextCompound(valueCheck, fields.GetField(nameof(request.Parameter.Value)), DefaultCode.Invalid)
				.NextCompound(request.Parameter.Usage.IsValidUsage, fields.GetField(nameof(request.Parameter.Usage)))
				.NextCompound(additionalDaysUntilDueCheck, fields.GetField(nameof(request.Parameter.AdditionalDaysUntilDue)), DefaultCode.Invalid)
				.NextNullCheck(currency, fields.GetField(nameof(request.Parameter.CurrencyId)), DefaultCode.NotFound)
				.NextCompound(canReceiveCheck, fields.GetField(nameof(request.Parameter.RecipientId)), DefaultCode.Invalid)
				.NextCompound(canCreateCheck, fields.GetField(nameof(request.AsAccountId)), DefaultCode.Invalid)
				.SetOnCriterionMet(successAction)
				.Evaluate();

			return retVal;
		}
		public async Task<IEncryptableResponse<SourceTransactionContractEntity>> GetTransactionSourcePreview(IAsAccountEncryptableRequest<CreateTransactionOfferParameter> request)
		{
			var response = new EncryptableResponse<SourceTransactionContractEntity>();

			async Task notNullRequest()
			{
				SourceTransactionContractEntity source = await CreateSourceTransactionContract(request, response.Validation);

				Lazy<IAccountEntity> account = GetAccountEntityLazily(request);
				UserEntity user = GetUserEntity(request);

				if (response.Validation.NoneInvalid)
				{
					response.Overwrite(source.CloneFor(account.Value));

					LogIfAccessingAsDelegate(user, "created transaction offer preview");
				}
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}
		public event ServiceEventHandler<ServiceEventArgs<TransactionOfferEntity>> OnTransactionOfferCreated;
		public event ServiceEventHandler<ServiceEventArgs<SourceTransactionContractEntity>> OnSourceTransactionContractCreated;
		public async Task<IResponse> CreateTransactionOffer(IAsAccountEncryptableRequest<CreateTransactionOfferParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				SourceTransactionContractEntity source = await CreateSourceTransactionContract(request, response.Validation);
				UserEntity user = GetUserEntity(request);

				if (response.Validation.NoneInvalid)
				{
					foreach (TargetTransactionContractEntity c in source.TargetTransactionContracts)
					{
						Connection.Insert(c);
					}
					Connection.Insert(source);

					Lazy<AccountSettingsEntityBase> recipientSettings = GetSettingsLazily<AccountSettingsEntityBase, AccountEntityBase>(source.Recipient);

					if (recipientSettings.Value.TransactionOfferLifetime > TimeSpan.Zero)
					{
						TransactionOfferEntity newOffer = new TransactionOfferEntity(source, recipientSettings.Value.TransactionOfferLifetime);
						newOffer.RefreshNow();
						Connection.Insert(newOffer);

						OnTransactionOfferCreated.Invoke(Session, new List<AccountEntityBase> { newOffer.Creditor, newOffer.Debtor }, newOffer.CloneAsT());

						LogIfAccessingAsDelegate(user, "created transaction offer");
					}
					else
					{
						Expose(source);

						OnSourceTransactionContractCreated.Invoke(Session, source.Creditor, source.CloneFor(source.Creditor));
						OnSourceTransactionContractCreated.Invoke(Session, source.Debtor, source.CloneFor(source.Debtor));

						LogIfAccessingAsDelegate(user, "created transaction source");
					}
					Connection.SaveChanges();
				}
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public event ServiceEventHandler<ServiceEventArgs<BookingEntity>> OnBookingCreated;
		public async Task<IResponse> CreateBooking(IAsAccountEncryptableRequest<CreateBookingParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<IAccountEntity> account = GetAccountEntityLazily<IAccountEntity>(request);

				Lazy<TargetTransactionContractEntity> targetContract = Connection.GetSingleLazily<TargetTransactionContractEntity>(request.Parameter.TargetTransactionId);
				Lazy<SourceTransactionContractEntity> sourceContract = Connection.GetSingleLazily<SourceTransactionContractEntity>(s => s.TargetTransactionContracts.Any(t => t.Id == targetContract.Value.Id));

				Lazy<Boolean> requestAccountIsCreditor = new Lazy<Boolean>(() => targetContract.Value.Creditor.Id == account.Value.Id);
				Lazy<Decimal> valueSubAccountBookingsSum = new Lazy<Decimal>(() =>
				{
					ICollection<BookingEntity> creditorBookings = targetContract.Value.CreditorBookings;

					ICollection<BookingEntity> debtorBookings = targetContract.Value.DebtorBookings;

					Decimal creditorBookingsSum = 0m;
					Decimal debtorBookingsSum = 0m;

					if (debtorBookings != null && debtorBookings.Any())
					{
						debtorBookingsSum = debtorBookings.Sum(b => b.Value);
					}
					if (creditorBookings != null && creditorBookings.Any())
					{
						creditorBookingsSum = creditorBookings.Sum(b => b.Value);
					}

					var accountBookingsSum = requestAccountIsCreditor.Value ? creditorBookingsSum : debtorBookingsSum;

					return targetContract.Value.Net - accountBookingsSum;
				});

				Boolean targetExposedCheck()
				{
					return !targetContract.Value.IsExposed;
				}
				Boolean targetBookedCheck()
				{
					return !targetContract.Value.IsBooked;
				}
				Boolean validBookingValueCheck()
				{
					return ValidateBookingValue(sourceContract.Value, targetContract.Value, account.Value, request.Parameter.Value);
				}
				Boolean notExceedingLeftoverCheck()
				{
					return request.Parameter.Value <= valueSubAccountBookingsSum.Value;
				}
				void successAction()
				{
					BookingEntity newBooking;

					if (requestAccountIsCreditor.Value)
					{
						newBooking = new BookingEntity(request.Parameter.Value);
						Connection.Insert(newBooking);
						targetContract.Value.CreditorBookings.Add(newBooking);
					}
					else
					{
						newBooking = new BookingEntity(request.Parameter.Value);
						Connection.Insert(newBooking);
						targetContract.Value.DebtorBookings.Add(newBooking);
					}

					OnBookingCreated.Invoke(Session, targetContract.Value, newBooking.CloneAsT());

					LogIfAccessingAsDelegate(user, "created booking");

					Connection.Update(targetContract);
					Connection.SaveChanges();

					if (targetContract.Value.IsBooked)
					{
						Expose(sourceContract.Value);
					}
				}

				await FirstValidateAsAccount(user, citizen, account, response.Validation)
					.NextManagerManagesProperty(account, targetContract, Connection, response.Validation.GetField(nameof(request.Parameter.TargetTransactionId)))
					.NextCompound(targetExposedCheck,
						DefaultCode.Invalid.SetMessage("The target requested has not been exposed yet."))
					.NextCompound(targetBookedCheck,
						DefaultCode.Invalid.SetMessage("The target requested is already fully booked."))
					.NextCompound(validBookingValueCheck,
						response.Validation.GetField(nameof(request.Parameter.Value)),
						DefaultCode.Invalid.SetMessage("The value requested is not valid."))
					.NextCompound(notExceedingLeftoverCheck,
						DefaultCode.Invalid.SetMessage("The value requested exceeds the value left to book."))
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}
		public event ServiceEventHandler<ServiceEventArgs<TransactionOfferEntity>> OnTransactionOfferAnswered;
		public async Task<IResponse> AnswerTransactionOffer(IAsAccountEncryptableRequest<AnswerTransactionOfferParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				GetService<IEventfulManageExpirantsService>().DeleteExpirants<TransactionOfferEntity>();

				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<IAccountEntity> account = GetAccountEntityLazily<IAccountEntity>(request);

				Lazy<TransactionOfferEntity> offer = Connection.GetSingleLazily<TransactionOfferEntity>(request.Parameter.TransactionOfferId);

				Boolean answerCheck()
				{
					return request.Parameter.Answer != TransactionOfferAnswer.None;
				}
				async Task successAction()
				{
					if (account.Value.Id == offer.Value.Creator.Id)
					{
						Boolean creatorAnswerCheck()
						{
							return offer.Value.CreatorConfirmation == TransactionOfferAnswer.None;
						}
						void creatorAnswerSuccessAction()
						{
							offer.Value.CreatorConfirmation = request.Parameter.Answer;
						}

						await FirstCompound(creatorAnswerCheck,
							   response.Validation.GetField(nameof(request.Parameter.Answer)),
							   DefaultCode.Invalid)
							.SetOnCriterionMet(creatorAnswerSuccessAction)
							.Evaluate();
					}
					else
					{
						Boolean recipientAnswerCheck()
						{
							return offer.Value.RecipientAnswer == TransactionOfferAnswer.None;
						}
						void recipientAnswerSuccessAction()
						{
							offer.Value.RecipientAnswer = request.Parameter.Answer;
						}

						await FirstCompound(recipientAnswerCheck,
							 response.Validation.GetField(nameof(request.Parameter.Answer)),
							 DefaultCode.Invalid)
							.SetOnCriterionMet(recipientAnswerSuccessAction)
							.Evaluate();
					}
					Connection.Update(offer);
					Connection.SaveChanges();

					OnTransactionOfferAnswered.Invoke(Session, offer.Value, offer.Value.CloneAsT());

					LogIfAccessingAsDelegate(user, "answered transaction offer");

					if (offer.Value.RecipientAnswer == TransactionOfferAnswer.Accepted && offer.Value.CreatorConfirmation == TransactionOfferAnswer.Accepted)
					{
						Expose(offer.Value.SourceTransactionContract);

						OnSourceTransactionContractCreated.Invoke(Session, offer.Value.SourceTransactionContract.Creditor, offer.Value.SourceTransactionContract.CloneFor(offer.Value.SourceTransactionContract.Creditor));
						OnSourceTransactionContractCreated.Invoke(Session, offer.Value.SourceTransactionContract.Debtor, offer.Value.SourceTransactionContract.CloneFor(offer.Value.SourceTransactionContract.Debtor));

						LogIfAccessingAsDelegate(user, "created transaction source");

						Connection.Delete(offer.Value);
						Connection.SaveChanges();
					}
					else if (offer.Value.RecipientAnswer == TransactionOfferAnswer.Rejected || offer.Value.CreatorConfirmation == TransactionOfferAnswer.Rejected)
					{
						Connection.Delete(offer.Value.SourceTransactionContract);
						Connection.Delete(offer.Value);
						Connection.SaveChanges();
					}
				}

				await FirstValidateAsAccount(user, citizen, account, response.Validation)
					.NextManagerManagesProperty(account, offer, Connection, response.Validation.GetField(nameof(request.Parameter.TransactionOfferId)))
					.NextCompound(answerCheck,
						response.Validation.GetField(nameof(request.Parameter.Answer)),
						DefaultCode.Invalid.SetMessage("The answer can not be none."))
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}
		public async Task<IEncryptableResponse<SourceTransactionContractEntity>> GetSourceTransactionContract(IAsAccountEncryptableRequest<GetSourceTransactionContractParameter> request)
		{
			var response = new EncryptableResponse<SourceTransactionContractEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<IAccountEntity> account = GetAccountEntityLazily(request);
				Lazy<SourceTransactionContractEntity> source = Connection.GetSingleLazily<SourceTransactionContractEntity>(request.Parameter.SourceTransactionContractId);

				void successAction()
				{
					response.Overwrite(source.Value.CloneFor(account.Value));

					LogIfAccessingAsDelegate(user, "retrieved transaction source");
				}

				await FirstValidateAsAccount(user, citizen, account, response.Validation)
					.NextManagerManagesProperty(account, source, Connection, response.Validation.GetField(nameof(request.Parameter.SourceTransactionContractId)))
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}
		public async Task<IEncryptableResponse<TargetTransactionContractEntity>> GetTargetTransactionContract(IAsAccountEncryptableRequest<GetTargetTransactionContractParameter> request)
		{
			var response = new EncryptableResponse<TargetTransactionContractEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<IAccountEntity> account = GetAccountEntityLazily(request);
				Lazy<TargetTransactionContractEntity> target = Connection.GetSingleLazily<TargetTransactionContractEntity>(request.Parameter.TargetTransactionContractId);

				void successAction()
				{
					response.Overwrite(target.Value.CloneAsT());

					LogIfAccessingAsDelegate(user, "retrieved transaction target");
				}

				await FirstValidateAsAccount(user, citizen, account, response.Validation)
					.NextManagerManagesProperty(account, target, Connection, response.Validation.GetField(nameof(request.Parameter.TargetTransactionContractId)))
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}
		public async Task<IEncryptableResponse<CurrencyEntity>> GetCurrency(GetCurrencyParameter request)
		{
			var response = new EncryptableResponse<CurrencyEntity>();

			async Task notNullRequest()
			{
				CurrencyEntity currency = Connection.GetSingle<CurrencyEntity>(request.CurrencyId);

				void setData()
				{
					response.Overwrite(currency.CloneAsT());
				}

				await FirstNullCheck(currency,
						response.Validation.GetField(nameof(request.CurrencyId)),
						DefaultCode.NotFound.SetMessage("The currency requested could not be found."))
					.SetOnCriterionMet(setData)
					.Evaluate();
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}
		public async Task<IGetPaginatedEncryptableResponse<CurrencyEntity>> GetCurrencies(IGetPaginatedRequest<GetCurrenciesParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<CurrencyEntity>();

			async Task notNullRequest()
			{
				var data = GetCurrencies();
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
		public event ServiceEventHandler<ServiceEventArgs<CurrencyEntity>> OnCurrencyCreated;
		public async Task<IResponse> CreateCurrency(IAsUserRequest<CreateCurrencyParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				var name = request.Parameter.Name.ToLower();
				Lazy<CurrencyEntity> duplicate = Connection.GetFirstLazily<CurrencyEntity>(c => c.Name.ToLower().Equals(name));

				Boolean roleCheck()
				{
					return user.IsInRole(Settings.ADMIN_ROLE);
				}

				void successAction()
				{
					CurrencyEntity newCurrency = new CurrencyEntity(user, request.Parameter.Name, request.Parameter.PluralName, request.Parameter.IngameTax);
					Connection.Insert(newCurrency);

					var dictionaries = Connection.Query<CurrencyBoolDictionaryEntity>();

					dictionaries.ForEach(d =>
					{
						Connection.Update(d);
						d.TryAddValue(newCurrency);
					});
					Connection.SaveChanges();
					IQueryable<Guid> ids = Connection.Query<UserSessionEntity>().Where(s => s.IsLoggedIn).Select(s => s.HubId).Concat(dictionaries.Select(d => d.HubId));
					OnCurrencyCreated.Invoke(Session, ids, newCurrency.CloneAsT());
				}

				await FirstValidateAsUser(user, response.Validation)
					.NextCompound(roleCheck,
						response.Validation.GetField(nameof(request)),
						DefaultCode.Unauthorized.SetMessage("The user requested is not authorized to create currencies."))
					.NextNullCheck(duplicate,
						response.Validation.GetField(nameof(request.Parameter.Name)),
						DefaultCode.Unauthorized.SetMessage("The currency requested has already been created."))
					.InvertCriterion()
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}
		public event ServiceEventHandler<ServiceEventArgs<CurrencyEntity>> OnCurrencyToggled;
		public async Task<IResponse> ToggleCurrency(IAsUserEncryptableRequest<ToggleCurrencyParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CurrencyEntity> currency = Connection.GetSingleLazily<CurrencyEntity>(request.Parameter.CurrencyId);

				Boolean roleCheck()
				{
					return user.IsInRole(Settings.ADMIN_ROLE);
				}
				void successAction()
				{
					currency.Value.IsActive = request.Parameter.IsActive;
					Connection.Update(currency.Value);
					Connection.SaveChanges();

					OnCurrencyToggled.Invoke(Session, currency.Value, currency.Value.CloneAsT());
				}

				await FirstValidateAsUser(user, response.Validation)
					.NextCompound(roleCheck,
						response.Validation.GetField(nameof(request.AsUserId)),
						DefaultCode.Unauthorized.SetMessage("The user requested is not authorized to toggle currencies."))
					.NextNullCheck(currency,
						response.Validation.GetField(nameof(request.Parameter.CurrencyId)),
						DefaultCode.NotFound.SetMessage("The currency requested could not be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}
		public event ServiceEventHandler<ServiceEventArgs> OnCurrencyDeleted;
		public async Task<IResponse> DeleteCurrency(IAsUserEncryptableRequest<DeleteCurrencyParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CurrencyEntity> currency = Connection.GetSingleLazily<CurrencyEntity>(request.Parameter.CurrencyId);

				Boolean roleCheck()
				{
					return user.IsInRole(Settings.ADMIN_ROLE);
				}
				void successAction()
				{
					Connection.Delete(currency.Value);
					Connection
						   .Query<CurrencyBoolDictionaryEntity>()
						   .ToList()
						   .ForEach(d =>
						   {
							   d.TryRemoveValue(currency.Value);
							   Connection.Update(d);
						   });
					Connection.SaveChanges();

					OnCurrencyDeleted.Invoke(currency.Value);
				}

				await FirstValidateAsUser(user, response.Validation)
					.NextCompound(roleCheck,
						response.Validation.GetField(nameof(request.AsUserId)),
						DefaultCode.Unauthorized.SetMessage("The user requested is not authorized to delete currencies."))
					.NextNullCheck(currency,
						response.Validation.GetField(nameof(request.Parameter.CurrencyId)),
						DefaultCode.NotFound.SetMessage("The currency requested could not be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}
		public event ServiceEventHandler<ServiceEventArgs<SourceTransactionContractEntity>> OnEqualizationTransactionCreated;
		public async Task<IResponse> CreateEqualizationTransaction(IAsAccountRequest request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<IVirtualAccountEntity> account = GetAccountEntityLazily<IVirtualAccountEntity>(request);

				void successAction()
				{
					throw new NotImplementedException("Equalizing depends on middleman logic.");

					LogIfAccessingAsDelegate(user, "created equalizing transaction source");
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
		public event ServiceEventHandler<ServiceEventArgs<TransactionOfferEntity>> OnTransactionOfferManipulated;
		public async Task<IResponse> ManipulateTransactionOffer(IAsAccountEncryptableRequest<ManipulateTransactionOfferParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<VirtualAccountEntityBase> account = GetAccountEntityLazily<VirtualAccountEntityBase>(request);
				Lazy<TransactionOfferEntity> offer = Connection.GetSingleLazily<TransactionOfferEntity>(request.Parameter.TransactionOfferId);
				Lazy<DepositAccountReferenceEntity> forwardingReference = new Lazy<DepositAccountReferenceEntity>(() => account.Value.DepositReferences.SingleOrDefault(r => r.Id == request.Parameter.ForwardingAccountReferenceId));
				Lazy<ICollection<DepositAccountReferenceEntity>> depositReferences = new Lazy<ICollection<DepositAccountReferenceEntity>>(() => request.Parameter.DepositAccountReferencesIds.Select(id => account.Value.DepositReferences.SingleOrDefault(r => r.Id == id)).Distinct().ToList());

				Boolean depositReferencesCheck()
				{
					return depositReferences.Value.All(r => r != null);
				}
				void successAction()
				{
					ManipulateTransactionContractOffer(account.Value,
																								 forwardingReference.Value,
																								 depositReferences.Value,
																								 offer.Value);

					OnTransactionOfferManipulated.Invoke(Session, offer.Value, offer.Value.CloneAsT());

					LogIfAccessingAsDelegate(user, "manipulated transaction offer");
				}

				await FirstValidateAsAccount(user, citizen, account, response.Validation)
					.NextManagerManagesProperty(account, offer, Connection, response.Validation.GetField(nameof(request.Parameter.TransactionOfferId)))
					.NextNullCheck(forwardingReference,
						response.Validation.GetField(nameof(request.Parameter.ForwardingAccountReferenceId)),
						DefaultCode.NotFound.SetMessage("The forwarding reference requested could not be found."))
					.NextCompound(depositReferencesCheck, 
						response.Validation.GetField(nameof(request.Parameter.DepositAccountReferencesIds)),
						DefaultCode.PartiallyNotFound.SetMessage("Not all of the deposit references requested could be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}
		public async Task<IGetPaginatedEncryptableResponse<TransactionOfferEntity>> GetTransactionOffers(IGetPaginatedAsAccountRequest<GetTransactionOffersParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<TransactionOfferEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<IAccountEntity> account = GetAccountEntityLazily(request);

				async Task successAction()
				{
					var data = Connection.Query<TransactionOfferEntity>()
						.Where(s => s.Creditor.Id == account.Value.Id || s.Debtor.Id == account.Value.Id)
						.FilterTransactions<TransactionOfferEntity, AccountEntityBase, AccountEntityBase, AccountEntityBase, AccountEntityBase>(request.Parameter.FilterProperty, request.Parameter.FilterComparator, request.Parameter.FilterValue)
						.OrderTransactions<TransactionOfferEntity, AccountEntityBase, AccountEntityBase, AccountEntityBase, AccountEntityBase>(request.Parameter.OrderByProperty, request.Parameter.OrderDescending);

					void setData()
					{
						response.LastPage = data.GetPageCount(request.PerPage) - 1;
						response.Data = data.Paginate(request.PerPage, request.Page).Select(a => a.CloneAsT()).ToList();

						LogIfAccessingAsDelegate(user, "retrieved transaction offers");
					}

					await CachedCriterionChain.Cache.Get()
						.ThisValidatePagination(request, data, response.Validation)
						.SetOnCriterionMet(setData)
						.Evaluate();
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
		public async Task<IGetPaginatedEncryptableResponse<SourceTransactionContractEntity>> GetSourceTransactionContracts(IGetPaginatedAsAccountRequest<GetSourceTransactionContractsParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<SourceTransactionContractEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<IAccountEntity> account = GetAccountEntityLazily(request);

				async Task successAction()
				{
					var data = Connection.Query<SourceTransactionContractEntity>()
						.Where(s => (s.Creditor.Id == account.Value.Id || s.Debtor.Id == account.Value.Id) && s.IsExposed)
						.FilterSourceTransactions(account.Value, request.Parameter.FilterProperty, request.Parameter.FilterComparator, request.Parameter.FilterValue)
						.OrderSourceTransactions(account.Value, request.Parameter.OrderByProperty, request.Parameter.OrderDescending);

					void setData()
					{
						response.LastPage = data.GetPageCount(request.PerPage) - 1;
						response.Data = data.Paginate(request.PerPage, request.Page).Select(a => a.CloneAsT()).ToList();

						LogIfAccessingAsDelegate(user, "retrieved transaction sources");
					}

					await CachedCriterionChain.Cache.Get()
						.ThisValidatePagination(request, data, response.Validation)
						.SetOnCriterionMet(setData)
						.Evaluate();
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
		public async Task<IGetPaginatedEncryptableResponse<TargetTransactionContractEntity>> GetTargetTransactionContracts(IGetPaginatedAsAccountRequest<GetTargetTransactionContractsParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<TargetTransactionContractEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<IAccountEntity> account = GetAccountEntityLazily(request);

				async Task successAction()
				{
					var data = Connection.Query<TargetTransactionContractEntity>()
						.Where(s => (s.Creditor.Id == account.Value.Id || s.Debtor.Id == account.Value.Id) && s.IsExposed)
						.FilterTargetTransactions(account.Value, request.Parameter.FilterProperty, request.Parameter.FilterComparator, request.Parameter.FilterValue)
						.OrderTargetTransactions(account.Value, request.Parameter.OrderByProperty, request.Parameter.OrderDescending);

					void setData()
					{
						response.LastPage = data.GetPageCount(request.PerPage) - 1;
						response.Data = data.Paginate(request.PerPage, request.Page).Select(a => a.CloneAsT()).ToList();

						LogIfAccessingAsDelegate(user, "retrieved transaction targets");
					}

					await CachedCriterionChain.Cache.Get()
						.ThisValidatePagination(request, data, response.Validation)
						.SetOnCriterionMet(setData)
						.Evaluate();
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

		public Boolean SuperficialPartnersRelationshipIsReal(IAccountEntity partnerA, IAccountEntity partnerB)
		{
			return RealTransactionPartnersRelationships.Contains(GetSuperficialRelationship(partnerA, partnerB));
		}
		public TransactionPartnersRelationship GetSuperficialRelationship(IAccountEntity creditor, IAccountEntity debtor)
		{
			if (creditor.Id == debtor.Id && creditor.Is<IVirtualAccountEntity>(Connection))
			{
				return TransactionPartnersRelationship.Equalizing;
			}
			if (creditor.Is<IVirtualAccountEntity>(Connection))
			{
				if (debtor.Is<IVirtualAccountEntity>(Connection))
				{
					return TransactionPartnersRelationship.VirtualToVirtual;
				}
				else if (debtor.Is<RealAccountEntity>(Connection))
				{
					return TransactionPartnersRelationship.RealToVirtual;
				}
			}
			else if (creditor.Is<RealAccountEntity>(Connection))
			{
				if (debtor is IVirtualAccountEntity)
				{
					return TransactionPartnersRelationship.VirtualToReal;
				}
				else if (debtor.Is<RealAccountEntity>(Connection))
				{
					return TransactionPartnersRelationship.RealToReal;
				}
			}
			return TransactionPartnersRelationship.None;
		}
		public void UpdateDepositBalanceIfBooked(SourceTransactionContractEntity source, TargetTransactionContractEntity target)
		{
			if (target.IsBooked)
			{
				void update(AccountEntityBase account, RealAccountEntity referencedAccount, Decimal value)
				{
					GetService<IEventfulAccountService>().UpdateDepositBalance(account.As<IVirtualAccountEntity>(Connection), referencedAccount, value);
				};
				switch (target.Relationship)
				{

					case TransactionPartnersRelationship.DepositToForward:
						{
							update(source.Debtor, target.Debtor, (-1) * target.Gross);
							break;
						}
					case TransactionPartnersRelationship.ForwardToForward:
						{
							update(source.Debtor, target.Debtor, (-1) * target.Gross);
							update(source.Creditor, target.Creditor, target.Net);
							break;
						}
					case TransactionPartnersRelationship.ForwardToDeposit:
						{
							update(source.Creditor, target.Creditor, target.Net);
							break;
						}
					case TransactionPartnersRelationship.ForwardToReal:
						{
							update(source.Debtor, target.Debtor, (-1) * target.Gross);
							break;
						}
					case TransactionPartnersRelationship.RealToForward:
						{
							update(source.Creditor, target.Creditor, target.Net);
							break;
						}
					case TransactionPartnersRelationship.EqualizingDepositToForward:
						{
							update(source.Debtor, target.Debtor, (-1) * target.Gross);
							update(source.Creditor, target.Creditor, target.Net);
							break;
						}
					case TransactionPartnersRelationship.EqualizingForwardToDeposit:
						{
							update(source.Debtor, target.Debtor, (-1) * target.Gross);
							update(source.Creditor, target.Creditor, target.Net);
							break;
						}
				}
			}
		}
		public SourceTransactionContractEntity CreateSourceTransactionContract<TCreditor, TDebtor, TCreator, TRecipient>(TCreditor creditor,
									   TDebtor debtor,
									   TCreator creator,
									   TRecipient recipient,
									   Decimal net,
									   CurrencyEntity currency,
									   String usage,
									   ICollection<TagEntity> tags,
									   TimeSpan additionalUntilDue)
			where TCreditor : AccountEntityBase
			where TDebtor : AccountEntityBase
			where TCreator : AccountEntityBase
			where TRecipient : AccountEntityBase
		{
			if (net < 1)
			{
				throw new InvalidOperationException("Net must be positive.");
			}

			TransactionPartnersRelationship sourceRelationship = GetSuperficialRelationship(creditor, debtor);
			List<TargetTransactionContractEntity> targetTransactionContracts = new List<TargetTransactionContractEntity> { };

			IEnumerable<DepositAccountReferenceEntity> getActiveDeposits(IVirtualAccountEntity account)
			{
				return account.DepositReferences.Where(r => r.IsActive && r.Currency.Id == currency.Id);
			};

			RealAccountEntity getForwardingAccount(IVirtualAccountEntity account)
			{
				IEnumerable<DepositAccountReferenceEntity> forwardings = getActiveDeposits(account).Where(r => r.UseAsForwarding);
				return forwardings.ElementAt(new Random().Next(0, forwardings.Count() - 1)).ReferencedAccount;
			};

			if (sourceRelationship == TransactionPartnersRelationship.Equalizing)
			{
				throw new NotImplementedException("Equalizing depends on middleman logic.");
			}
			else if (sourceRelationship != TransactionPartnersRelationship.None)
			{
				Lazy<VirtualAccountEntityBase> virtualCreditor = Connection.GetFirstLazily<VirtualAccountEntityBase>(creditor.Id);
				Lazy<VirtualAccountSettingsEntityBase> virtualCreditorSettings = new Lazy<VirtualAccountSettingsEntityBase>(() => GetSettings<VirtualAccountSettingsEntityBase, AccountEntityBase>(virtualCreditor.Value));

				Lazy<VirtualAccountEntityBase> virtualDebtor = Connection.GetFirstLazily<VirtualAccountEntityBase>(debtor.Id);
				Lazy<VirtualAccountSettingsEntityBase> virtualDebtorSettings = new Lazy<VirtualAccountSettingsEntityBase>(() => GetSettings<VirtualAccountSettingsEntityBase, AccountEntityBase>(virtualDebtor.Value));

				Lazy<RealAccountEntity> realCreditor = Connection.GetFirstLazily<RealAccountEntity>(creditor.Id);
				Lazy<RealAccountSettingsEntity> realCreditorSettings = new Lazy<RealAccountSettingsEntity>(() => GetSettings<RealAccountSettingsEntity, AccountEntityBase>(realCreditor.Value));

				Lazy<RealAccountEntity> realDebtor = Connection.GetFirstLazily<RealAccountEntity>(debtor.Id);
				Lazy<RealAccountSettingsEntity> realDebtorSettings = new Lazy<RealAccountSettingsEntity>(() => GetSettings<RealAccountSettingsEntity, AccountEntityBase>(realDebtor.Value));

				Lazy<CBData.Abstractions.IAccountSettingsEntity> recipientSettings = GetSettingsLazily<IAccountSettingsEntity, AccountEntityBase>(recipient);

				additionalUntilDue += recipientSettings.Value.MinimumContractLifeSpan;

				List<Tuple<RealAccountEntity, Decimal>> getDepositCuts(IVirtualAccountEntity account, Decimal value)
				{
					List<Tuple<RealAccountEntity, Decimal>> retVal = new List<Tuple<RealAccountEntity, Decimal>> { };
					IEnumerable<DepositAccountReferenceEntity> depositAccounts = getActiveDeposits(account);
					Decimal absoluteLimit = depositAccounts.Sum(d => d.CalculatedAbsoluteLimit);
					Decimal sum = 0M;
					foreach (DepositAccountReferenceEntity d in depositAccounts)
					{
						RealAccountEntity referencedAccount = d.ReferencedAccount;
						Decimal relativeLimit = d.CalculatedAbsoluteLimit / absoluteLimit;
						Decimal val = relativeLimit * value;
						if ((sum + val) > value)
						{
							val = value - sum;
						}
						val = val.RoundCIG();
						retVal.Add(new Tuple<RealAccountEntity, Decimal>(referencedAccount, val));
						sum += val;
					}
					return retVal;
				};

				switch (sourceRelationship)
				{
					case TransactionPartnersRelationship.RealToReal:
						{
							targetTransactionContracts.Add(new TargetTransactionContractEntity(creator,
																							recipient,
																							realCreditor.Value,
																							realDebtor.Value,
																						   usage,
																						   currency.IngameTax,
																						   currency,
																						   net,
																						   TransactionPartnersRelationship.RealToReal,
																						   additionalUntilDue));
							break;
						}
					case TransactionPartnersRelationship.RealToVirtual:
						{
							RealAccountEntity creditorForwardingAccount = getForwardingAccount(virtualCreditor.Value);
							List<Tuple<RealAccountEntity, Decimal>> creditorDepositCuts = getDepositCuts(virtualCreditor.Value, net);

							foreach (Tuple<RealAccountEntity, Decimal> cutData in creditorDepositCuts)
							{
								targetTransactionContracts.Add(new TargetTransactionContractEntity(creator,
																								   recipient,
																								   cutData.Item1,
																								   creditorForwardingAccount,
																								   CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																								   currency.IngameTax,
																								   currency,
																								   cutData.Item2,
																								   TransactionPartnersRelationship.ForwardToDeposit,
																								   virtualCreditorSettings.Value.DepositForwardLifeSpan));
							}
							targetTransactionContracts.Add(new TargetTransactionContractEntity(creator,
																							   recipient,
																							   creditorForwardingAccount,
																							   realDebtor.Value,
																							   CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																							   currency.IngameTax,
																							   currency,
																							   targetTransactionContracts.Sum(c => c.Gross),
																							   TransactionPartnersRelationship.RealToForward,
																							   additionalUntilDue));
							break;
						}
					case TransactionPartnersRelationship.VirtualToReal:
						{
							RealAccountEntity debtorForwardingAccount = getForwardingAccount(virtualDebtor.Value);
							targetTransactionContracts.Add(new TargetTransactionContractEntity(creator,
																							   recipient,
																							   realCreditor.Value,
																							   debtorForwardingAccount,
																							   CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																							   currency.IngameTax,
																							   currency,
																							   net,
																							   TransactionPartnersRelationship.ForwardToReal,
																							   additionalUntilDue));
							List<Tuple<RealAccountEntity, Decimal>> debtorDepositCuts = getDepositCuts(virtualDebtor.Value, targetTransactionContracts.Sum(c => c.Gross));
							foreach (Tuple<RealAccountEntity, Decimal> cutData in debtorDepositCuts)
							{
								targetTransactionContracts.Add(new TargetTransactionContractEntity(creator,
																							   recipient,
																								   debtorForwardingAccount,
																								   cutData.Item1,
																								   CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																								   currency.IngameTax,
																								   currency,
																								   cutData.Item2,
																								   TransactionPartnersRelationship.DepositToForward,
																								   virtualDebtorSettings.Value.DepositForwardLifeSpan));
							}
							break;
						}
					case TransactionPartnersRelationship.VirtualToVirtual:
						{
							RealAccountEntity debtorForwardingAccount = getForwardingAccount(virtualDebtor.Value);
							RealAccountEntity creditorForwardingAccount = getForwardingAccount(virtualCreditor.Value);

							List<Tuple<RealAccountEntity, Decimal>> creditorDepositCuts = getDepositCuts(virtualCreditor.Value, net);
							foreach (Tuple<RealAccountEntity, Decimal> cutData in creditorDepositCuts)
							{
								targetTransactionContracts.Add(new TargetTransactionContractEntity(creator,
																							   recipient,
																								   cutData.Item1,
																								   creditorForwardingAccount,
																								   CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																								   currency.IngameTax,
																								   currency,
																								   cutData.Item2,
																								   TransactionPartnersRelationship.ForwardToDeposit,
																								   virtualCreditorSettings.Value.DepositForwardLifeSpan));
							}

							targetTransactionContracts.Add(new TargetTransactionContractEntity(creator,
																							   recipient,
																							   creditorForwardingAccount,
																							   debtorForwardingAccount,
																							   CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																							   currency.IngameTax,
																							   currency,
																							   targetTransactionContracts.Sum(c => c.Gross),
																							   TransactionPartnersRelationship.ForwardToForward,
																							   additionalUntilDue));

							List<Tuple<RealAccountEntity, Decimal>> debtorDepositCuts = getDepositCuts(virtualDebtor.Value, targetTransactionContracts.Where(t => t.Relationship == TransactionPartnersRelationship.ForwardToForward).Sum(c => c.Gross));
							foreach (Tuple<RealAccountEntity, Decimal> cutData in debtorDepositCuts)
							{
								targetTransactionContracts.Add(new TargetTransactionContractEntity(creator,
																								   recipient,
																								   debtorForwardingAccount,
																								   cutData.Item1,
																								   CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																								   currency.IngameTax,
																								   currency,
																								   cutData.Item2,
																								   TransactionPartnersRelationship.DepositToForward,
																								   virtualDebtorSettings.Value.DepositForwardLifeSpan));
							}
							break;
						}
				}
			}
			else
			{
				return null;
			}
			return new SourceTransactionContractEntity(creator,
												 recipient,
												 creditor,
												 debtor,
												 usage,
												 currency,
												 net,
												 net + targetTransactionContracts.Sum(t => (t.Gross - t.Net).RoundCIG()),
												 sourceRelationship)
			{
				TargetTransactionContracts = targetTransactionContracts,
				Tags = tags
			};

		}
		public void ManipulateTransactionContractOffer(VirtualAccountEntityBase manipulator,
													  DepositAccountReferenceEntity forwardingAccountReference,
													  ICollection<DepositAccountReferenceEntity> depositAccountReferences,
													  TransactionOfferEntity offer)
		{
			ICollection<TargetTransactionContractEntity> targets = offer.SourceTransactionContract.TargetTransactionContracts;
			CurrencyEntity currency = offer.Currency;

			AccountEntityBase newRecipient = manipulator.Id == offer.Creator.Id ? offer.Recipient : offer.Creator;

			if (!(manipulator.DepositReferences.Any(f => f.Id == forwardingAccountReference.Id) && depositAccountReferences.All(d1 => d1.Currency == currency && manipulator.DepositReferences.Any(d2 => d2.Id == d1.Id))))
			{
				throw new InvalidOperationException("invalid manipulation attempt");
			}

			Boolean manipulatorIsCreditor = manipulator.Id == offer.Creditor.Id;

			List<Tuple<RealAccountEntity, Decimal>> getManipulatorCuts(Decimal value)
			{
				List<Tuple<RealAccountEntity, Decimal>> depositCuts = new List<Tuple<RealAccountEntity, Decimal>> { };
				Decimal absoluteLimit = depositAccountReferences.Sum(d => d.CalculatedAbsoluteLimit);
				foreach (DepositAccountReferenceEntity d in depositAccountReferences)
				{
					depositCuts.Add(new Tuple<RealAccountEntity, Decimal>(Connection.GetFirst<RealAccountEntity>(d.ReferencedAccount.Id), (d.CalculatedAbsoluteLimit / absoluteLimit) * value));
				}
				return depositCuts;
			};

			List<Tuple<RealAccountEntity, Decimal>> getOtherCuts(IVirtualAccountEntity account, Decimal value)
			{
				List<Tuple<RealAccountEntity, Decimal>> depositCuts = new List<Tuple<RealAccountEntity, Decimal>> { };
				ICollection<DepositAccountReferenceEntity> baseDepositAccountReferences = account.DepositReferences;
				IEnumerable<DepositAccountReferenceEntity> otherDepositAccountReferences = manipulatorIsCreditor ?
				targets
				.Where(t => t.Relationship == TransactionPartnersRelationship.DepositToForward)
				.Select(t => baseDepositAccountReferences
					.Single(d => d.ReferencedAccount.Id == t.Debtor.Id)) :
				targets
				.Where(t => t.Relationship == TransactionPartnersRelationship.ForwardToDeposit)
				.Select(t => baseDepositAccountReferences
					.Single(d => d.ReferencedAccount.Id == t.Creditor.Id));

				Decimal absoluteLimit = otherDepositAccountReferences.Sum(d => d.CalculatedAbsoluteLimit);
				foreach (DepositAccountReferenceEntity d in otherDepositAccountReferences)
				{
					depositCuts.Add(new Tuple<RealAccountEntity, Decimal>(Connection.GetFirst<RealAccountEntity>(d.ReferencedAccount.Id), (d.CalculatedAbsoluteLimit / absoluteLimit) * value));
				}
				return depositCuts;
			};

			Lazy<IVirtualAccountSettingsEntity> manipulatorSettings = GetSettingsLazily<IVirtualAccountSettingsEntity, AccountEntityBase>(manipulator);

			RealAccountEntity manipulatorForwardingAccount = Connection.GetFirst<RealAccountEntity>(forwardingAccountReference.ReferencedAccount.Id);
			List<TargetTransactionContractEntity> newTargets = new List<TargetTransactionContractEntity> { };

			if (manipulatorIsCreditor)
			{
				if (offer.Debtor.Is<IVirtualAccountEntity>(out IVirtualAccountEntity virtualDebtor, Connection))
				{
					Lazy<CBData.Abstractions.IVirtualAccountSettingsEntity> debtorSettings = GetSettingsLazily<IVirtualAccountSettingsEntity, AccountEntityBase>((AccountEntityBase)virtualDebtor);

					getManipulatorCuts(offer.Net).ForEach(datum => newTargets.Add(new TargetTransactionContractEntity(manipulator,
																									   newRecipient,
																									   datum.Item1,
																									   manipulatorForwardingAccount,
																									   CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																									   currency.IngameTax,
																									   currency,
																									   datum.Item2,
																									   TransactionPartnersRelationship.ForwardToDeposit,
																									   manipulatorSettings.Value.DepositForwardLifeSpan)));
					TargetTransactionContractEntity previousFTFContract = targets.Single(t => t.Relationship == TransactionPartnersRelationship.ForwardToForward);
					RealAccountEntity debtorForwardingAccount = Connection.GetFirst<RealAccountEntity>(previousFTFContract.Debtor.Id);
					TargetTransactionContractEntity newFTFContract = new TargetTransactionContractEntity(manipulator,
															  newRecipient,
															  manipulatorForwardingAccount,
															  debtorForwardingAccount,
															  CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
															  currency.IngameTax,
															  currency,
															  newTargets.Sum(t => t.Gross),
															  TransactionPartnersRelationship.ForwardToForward,
															  previousFTFContract.LifeSpan);
					newTargets.Add(newFTFContract);
					getOtherCuts(virtualDebtor, newFTFContract.Gross).ForEach(datum => newTargets.Add(new TargetTransactionContractEntity(manipulator,
																														   newRecipient,
																														   manipulatorForwardingAccount,
																														   datum.Item1,
																														   CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																														   currency.IngameTax,
																														   currency,
																														   datum.Item2,
																														   TransactionPartnersRelationship.DepositToForward,
																														   debtorSettings.Value.DepositForwardLifeSpan)));
				}
				else if (offer.Debtor is RealAccountEntity realDebtor)
				{
					getManipulatorCuts(offer.Net).ForEach(datum => newTargets.Add(new TargetTransactionContractEntity(manipulator,
																									   newRecipient,
																									   datum.Item1,
																									   manipulatorForwardingAccount,
																									   CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																									   currency.IngameTax,
																									   currency,
																									   datum.Item2,
																									   TransactionPartnersRelationship.ForwardToDeposit,
																									   manipulatorSettings.Value.DepositForwardLifeSpan)));
					TargetTransactionContractEntity previousRTFContract = targets.Single(t => t.Relationship == TransactionPartnersRelationship.RealToForward);
					TargetTransactionContractEntity newRTFContract = new TargetTransactionContractEntity(manipulator,
															  newRecipient,
															  manipulatorForwardingAccount,
															  realDebtor,
															  CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
															  currency.IngameTax,
															  currency,
															  newTargets.Sum(t => t.Gross),
															  TransactionPartnersRelationship.RealToForward,
															  previousRTFContract.LifeSpan);
					newTargets.Add(newRTFContract);
				}
			}
			else
			{
				if (offer.Creditor.Is<IVirtualAccountEntity>(out IVirtualAccountEntity virtualCreditor, Connection))
				{
					Lazy<CBData.Abstractions.IVirtualAccountSettingsEntity> creditorSettings = GetSettingsLazily<IVirtualAccountSettingsEntity, AccountEntityBase>((AccountEntityBase)virtualCreditor);
					TargetTransactionContractEntity previousFTFContract = targets.Single(t => t.Relationship == TransactionPartnersRelationship.ForwardToForward);
					RealAccountEntity creditorForwardingAccount = Connection.GetFirst<RealAccountEntity>(previousFTFContract.Creditor.Id);
					getOtherCuts(virtualCreditor, offer.Net).ForEach(datum => newTargets.Add(new TargetTransactionContractEntity(manipulator,
																												  newRecipient,
																												  datum.Item1,
																												  creditorForwardingAccount,
																												  CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																												  currency.IngameTax,
																												  currency,
																												  datum.Item2,
																												  TransactionPartnersRelationship.DepositToForward,
																												  creditorSettings.Value.DepositForwardLifeSpan)));

					TargetTransactionContractEntity newFTFContract = new TargetTransactionContractEntity(manipulator,
															  newRecipient,
															  creditorForwardingAccount,
															  manipulatorForwardingAccount,
															  CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
															  currency.IngameTax,
															  currency,
															  newTargets.Sum(t => t.Gross),
															  TransactionPartnersRelationship.ForwardToForward,
															  previousFTFContract.LifeSpan);
					newTargets.Add(newFTFContract);

					getManipulatorCuts(offer.Net).ForEach(datum => newTargets.Add(new TargetTransactionContractEntity(manipulator,
																									   newRecipient,
																									   manipulatorForwardingAccount,
																									   datum.Item1,
																									   CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																									   currency.IngameTax,
																									   currency,
																									   datum.Item2,
																									   TransactionPartnersRelationship.DepositToForward,
																									   creditorSettings.Value.DepositForwardLifeSpan)));
				}
				else if (offer.Creditor is RealAccountEntity realCreditor)
				{
					TargetTransactionContractEntity previousFTRContract = targets.Single(t => t.Relationship == TransactionPartnersRelationship.ForwardToReal);
					TargetTransactionContractEntity newFTRContract = new TargetTransactionContractEntity(manipulator,
															  newRecipient,
															  realCreditor,
															  manipulatorForwardingAccount,
															  CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
															  currency.IngameTax,
															  currency,
															  offer.Net,
															  previousFTRContract.Relationship,
															  previousFTRContract.LifeSpan);
					newTargets.Add(newFTRContract);
					getManipulatorCuts(newFTRContract.Gross).ForEach(datum => newTargets.Add(new TargetTransactionContractEntity(manipulator,
																												  newRecipient,
																												  manipulatorForwardingAccount,
																												  datum.Item1,
																												  CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																												  currency.IngameTax,
																												  currency,
																												  datum.Item2,
																												  TransactionPartnersRelationship.DepositToForward,
																												  manipulatorSettings.Value.DepositForwardLifeSpan)));

				}
			}
			SourceTransactionContractEntity newSource = new SourceTransactionContractEntity(manipulator,
													   newRecipient,
													   offer.Creditor,
													   offer.Debtor,
													   offer.Usage,
													   offer.Currency,
													   offer.Net,
													   newTargets.Sum(c => c.Gross),
													   offer.SourceTransactionContract.Relationship)
			{
				TargetTransactionContracts = newTargets
			};
			offer.SourceTransactionContract
				.TargetTransactionContracts
				.ToList()
				.ForEach(t => Connection.Delete(t));
			Connection.Delete(offer.SourceTransactionContract);
			newSource
				.TargetTransactionContracts
				.ToList()
				.ForEach(t => Connection.Insert(t));
			Connection.Insert(newSource);
			offer.Creator = manipulator;
			offer.Recipient = newRecipient;
			offer.SourceTransactionContract = newSource;
			offer.CreatorConfirmation = TransactionOfferAnswer.None;
			offer.RecipientAnswer = TransactionOfferAnswer.None;
			Connection.Update(offer);
			Connection.SaveChanges();
		}
		public Boolean ValidateBookingValue(SourceTransactionContractEntity source, TargetTransactionContractEntity target, IAccountEntity bookingAccount, Decimal bookingValue)
		{
			if (bookingValue > target.Net)
			{
				return false;
			}
			switch (target.Relationship)
			{
				case TransactionPartnersRelationship.DepositToForward:
					{
						DepositAccountReferenceEntity reference = source
							.Debtor
							.As<IVirtualAccountEntity>(Connection)
							.DepositReferences
							.Single(r => r.ReferencedAccount.Equals(target.Debtor));
						return bookingValue <= reference.AbsoluteBalance;
					}
				case TransactionPartnersRelationship.ForwardToDeposit:
					{
						DepositAccountReferenceEntity reference = source
							.Creditor
							.As<IVirtualAccountEntity>(Connection)
							.DepositReferences
							.Single(r => r.ReferencedAccount.Equals(target.Creditor));
						return bookingValue <= reference.CalculatedAbsoluteLimit - reference.AbsoluteBalance;
					}
				default:
					{
						return true;
					}
			}
		}
		public SourceTransactionContractEntity CloneAsAccount(SourceTransactionContractEntity source, IAccountEntity account)
		{
			Boolean isCreditor = account.Id == source.Creditor.Id;
			Boolean isDebtor = account.Id == source.Debtor.Id;

			var retVal = source.CloneAsT();

			Boolean check(TargetTransactionContractEntity t)
			{
				return (source.Relationship == TransactionPartnersRelationship.RealToReal &&
							(isCreditor || isDebtor)) ||
						(source.Relationship == TransactionPartnersRelationship.RealToVirtual &&
							(isCreditor ||
							(isDebtor && t.Relationship == TransactionPartnersRelationship.RealToForward))) ||
						(source.Relationship == TransactionPartnersRelationship.VirtualToReal &&
							(isDebtor ||
							(isCreditor && t.Relationship == TransactionPartnersRelationship.ForwardToReal))) ||
						(source.Relationship == TransactionPartnersRelationship.VirtualToVirtual &&
							((isCreditor &&
								(t.Relationship == TransactionPartnersRelationship.ForwardToForward || t.Relationship == TransactionPartnersRelationship.ForwardToDeposit)) ||
							(isDebtor &&
								(t.Relationship == TransactionPartnersRelationship.ForwardToForward || t.Relationship == TransactionPartnersRelationship.DepositToForward)))) ||
						(source.Relationship == TransactionPartnersRelationship.Equalizing &&
							isCreditor);
			}

			retVal.TargetTransactionContracts = source.TargetTransactionContracts.Where(check).ToList();

			return retVal;
		}

		public ICollection<CurrencyEntity> GetCurrencies()
		{
			return Connection.Query<CurrencyEntity>().ToList();
		}

		public event ServiceEventHandler<ServiceEventArgs<TargetTransactionContractEntity>> OnTargetExposed;
		public void Expose(SourceTransactionContractEntity source)
		{
			DateTimeOffset now = DateTimeOffset.Now;

			if (!source.IsExposed)
			{
				source.Expose(now);
			}

			CBCommon.Components.TransactionContractExposureRuleBook ruleBook = CBCommon.Components.Statics.TransactionContractExposureRuleBookDictionary[source.Relationship];

			IEnumerable<TransactionPartnersRelationship> tier = ruleBook.GetSuccessors(source.Relationship);
			IEnumerable<TargetTransactionContractEntity> targets = source.TargetTransactionContracts.Where(t => tier.Any(r => r == t.Relationship));

			void expose()
			{
				targets.ToList().ForEach(exposeT);
				void exposeT(TargetTransactionContractEntity t)
				{
					if (!t.IsExposed)
					{
						t.Expose(now);
						List<Guid> recipients = new List<Guid>
						{
							source.HubId,
							t.HubId
						};
						OnTargetExposed.Invoke(Session,
							 recipients,
							t.CloneAsT());
					}

					UpdateDepositBalanceIfBooked(source, t);
				}
			}

			expose();

			while (targets.All(t => t.IsBooked) && !source.TargetTransactionContracts.All(t => t.IsBooked))
			{
				tier = ruleBook.GetSuccessors(targets.First().Relationship);
				targets = source.TargetTransactionContracts.Where(t => tier.Any(r => r == t.Relationship));
				expose();
			}

			Connection.Update(source);
			Connection.SaveChanges();
		}
	}
}
