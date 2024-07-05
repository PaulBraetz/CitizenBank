using CBApplication.Extensions;
using CBApplication.Requests.Abstractions;
using CBApplication.Services.Abstractions;
using CBCommon.Extensions;
using CBData.Abstractions;
using CBData.Entities;
using PBApplication.Context.Abstractions;
using PBApplication.Events;
using PBApplication.Extensions;
using PBApplication.Requests.Abstractions;
using PBApplication.Responses;
using PBApplication.Responses.Abstractions;
using PBApplication.Services.Abstractions;
using PBCommon.Extensions;
using PBCommon.Globalization;
using PBCommon.Validation;
using PBData.Entities;
using PBData.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CBApplication.Services.Abstractions.ITransactionService;
using static CBCommon.Enums.CitizenBankEnums;

namespace CBApplication.Services
{
	public class TransactionService : CBService, IEventfulTransactionService
	{
		public static readonly SimpleTransactionContractProperty DefaultTransactionContractFilterProperty = SimpleTransactionContractProperty.Created;

		public TransactionService(IServiceContext serviceContext) : base(serviceContext)
		{
			Observe<IEventfulTransactionService>(this);
		}

		private async Task<SourceTransactionContractEntity> CreateSourceTransactionContract(IAsAccountEncryptableRequest<CreateTransactionOfferParameter> request, IResponse response)
		{
			UserEntity user = GetUserEntity(request);
			CitizenEntity citizen = GetCitizenEntity(request);
			AccountEntityBase creator = GetAccountEntity<AccountEntityBase>(request);
			CurrencyEntity currency = Connection.GetSingle<CurrencyEntity>(request.Parameter.CurrencyId);
			AccountEntityBase recipient = Connection.GetSingle<AccountEntityBase>(request.Parameter.RecipientId);
			AccountEntityBase creditor = Connection.GetSingle<AccountEntityBase>(request.Parameter.CreditorId);
			AccountEntityBase debtor = Connection.GetSingle<AccountEntityBase>(request.Parameter.DebtorId);

			Lazy<IAccountSettingsEntity> creatorSettings = new(() => GetSettings<IAccountSettingsEntity>(creator));
			Lazy<IAccountSettingsEntity> recipientSettings = new(() => GetSettings<IAccountSettingsEntity>(recipient));

			SourceTransactionContractEntity retVal = null;

			Boolean creditorDebtorCheck()
			{
				return (creditor.Id == creator.Id && debtor.Id == recipient.Id) || (debtor.Id == creator.Id && creditor.Id == recipient.Id);
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
				return recipientSettings.Value.CanReceiveTransactionOffersFor[currency];
			}
			Boolean canCreateCheck()
			{
				return creatorSettings.Value.CanCreateTransactionOffersFor[currency];
			}
			async Task successAction()
			{
				TagEntity[] tags = request.Parameter.TagsTexts != null ? await Task.WhenAll(request.Parameter.TagsTexts.Select(t => GetService<IEventfulTagService>().GetTag(t))) : Array.Empty<TagEntity>();
				retVal = CreateSourceTransactionContract(creator,
					recipient,
					creditor,
					debtor,
					request.Parameter.Value,
					currency,
					request.Parameter.Usage,
					tags,
					TimeSpan.FromDays(request.Parameter.AdditionalDaysUntilDue));
			}

			await FirstValidateAsAccount(request, response)
				.NextNullCheck(creditor, ValidationField.Create(nameof(request.Parameter.CreditorId)), ValidationCode.NotFound)
				.NextNullCheck(debtor, ValidationField.Create(nameof(request.Parameter.DebtorId)))
				.NextNullCheck(recipient, ValidationField.Create(nameof(request.Parameter.RecipientId)))
				.NextCompound(creditorDebtorCheck, ValidationField.Miscellaneous, ValidationCode.Invalid)
				.NextCompound(valueCheck, ValidationField.Create(nameof(request.Parameter.Value)), ValidationCode.Invalid)
				.NextCompound(request.Parameter.Usage.IsValidUsage, ValidationField.Create(nameof(request.Parameter.Usage)))
				.NextCompound(additionalDaysUntilDueCheck, ValidationField.Create(nameof(request.Parameter.AdditionalDaysUntilDue)), ValidationCode.Invalid)
				.NextNullCheck(currency, ValidationField.Create(nameof(request.Parameter.CurrencyId)), ValidationCode.NotFound)
				.NextCompound(canReceiveCheck, ValidationField.Create(nameof(request.Parameter.RecipientId)), ValidationCode.Invalid)
				.NextCompound(canCreateCheck, ValidationField.Create(nameof(request.AsAccountId)), ValidationCode.Invalid)
				.SetOnCriterionMet(successAction)
				.Evaluate(response);

			return retVal;
		}
		public async Task<IEncryptableResponse<SourceTransactionContractEntity>> GetTransactionSourcePreview(IAsAccountEncryptableRequest<CreateTransactionOfferParameter> request)
		{
			var response = new EncryptableResponse<SourceTransactionContractEntity>();

			async Task notNullRequest()
			{
				SourceTransactionContractEntity source = await CreateSourceTransactionContract(request, response);

				if (response.Validation.NoneInvalid)
				{
					IAccountEntity account = GetAccountEntity(request);
					response.Overwrite(source.CloneFor(account));

					UserEntity user = GetUserEntity(request);
					LogIfAccessingAsDelegate(user, "created transaction offer preview");
				}
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}
		public event ServiceEventHandler<ServiceEventArgs<TransactionOfferEntity>> OnTransactionOfferCreated;
		public event ServiceEventHandler<ServiceEventArgs<SourceTransactionContractEntity>> OnSourceTransactionContractCreated;
		public async Task<IResponse> CreateTransactionOffer(IAsAccountEncryptableRequest<CreateTransactionOfferParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				SourceTransactionContractEntity source = await CreateSourceTransactionContract(request, response);

				if (response.Validation.NoneInvalid)
				{
					foreach (TargetTransactionContractEntity c in source.TargetTransactionContracts)
					{
						Connection.Insert(c);
					}
					Connection.Insert(source);

					IAccountSettingsEntity recipientSettings = GetSettings<IAccountSettingsEntity>(source.Recipient);

					UserEntity user = GetUserEntity(request);

					if (recipientSettings.TransactionOfferLifetime > TimeSpan.Zero)
					{
						TransactionOfferEntity newOffer = new(source, recipientSettings.TransactionOfferLifetime);
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
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}

		public event ServiceEventHandler<ServiceEventArgs<BookingEntity>> OnBookingCreated;
		public async Task<IResponse> CreateBooking(IAsAccountEncryptableRequest<CreateBookingParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				CitizenEntity citizen = GetCitizenEntity(request);
				IAccountEntity account = GetAccountEntity(request);

				TargetTransactionContractEntity targetContract = Connection.GetSingle<TargetTransactionContractEntity>(request.Parameter.TargetTransactionId);
				Lazy<SourceTransactionContractEntity> sourceContract = new(() => Connection.GetSingle<SourceTransactionContractEntity>(s => s.TargetTransactionContracts.Any(t => t.Id == targetContract.Id)));

				Boolean requestAccountIsCreditor()
				{
					return targetContract.Creditor.Id == account.Id;
				}
				Lazy<Decimal> valueSubAccountBookingsSum = new(() =>
				{
					ICollection<BookingEntity> creditorBookings = targetContract.CreditorBookings;

					ICollection<BookingEntity> debtorBookings = targetContract.DebtorBookings;

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

					var accountBookingsSum = requestAccountIsCreditor() ? creditorBookingsSum : debtorBookingsSum;

					return targetContract.Net - accountBookingsSum;
				});

				Boolean authorizedCheck()
				{
					return account.HoldsManagerRightImplicitlyRecursively(Connection, targetContract);
				}
				Boolean targetExposedCheck()
				{
					return !targetContract.IsExposed;
				}
				Boolean targetBookedCheck()
				{
					return !targetContract.IsBooked;
				}
				Boolean validBookingValueCheck()
				{
					return ValidateBookingValue(sourceContract.Value, targetContract, account, request.Parameter.Value);
				}
				Boolean notExceedingLeftoverCheck()
				{
					return request.Parameter.Value <= valueSubAccountBookingsSum.Value;
				}

				void successAction()
				{
					BookingEntity newBooking;

					if (requestAccountIsCreditor())
					{
						newBooking = new BookingEntity(request.Parameter.Value);
						Connection.Insert(newBooking);
						targetContract.CreditorBookings.Add(newBooking);
					}
					else
					{
						newBooking = new BookingEntity(request.Parameter.Value);
						Connection.Insert(newBooking);
						targetContract.DebtorBookings.Add(newBooking);
					}

					OnBookingCreated.Invoke(Session, targetContract, newBooking.CloneAsT());

					LogIfAccessingAsDelegate(user, "created booking");

					Connection.Update(targetContract);
					Connection.SaveChanges();

					if (targetContract.IsBooked)
					{
						Expose(sourceContract.Value);
					}
				}

				await FirstValidateAsAccount(request, response)
					.NextCompound(authorizedCheck,
						ValidationField.Create(nameof(request.Parameter.TargetTransactionId)),
						ValidationCode.Unauthorized)
					.NextCompound(targetExposedCheck,
						ValidationCode.Invalid.WithMessage("The target requested has not been exposed yet."))
					.InheritField()
					.NextCompound(targetBookedCheck,
						ValidationCode.Invalid.WithMessage("The target requested is already fully booked."))
					.InheritField()
					.NextCompound(validBookingValueCheck,
						ValidationField.Create(nameof(request.Parameter.Value)),
						ValidationCode.Invalid.WithMessage("The value requested is not valid."))
					.NextCompound(notExceedingLeftoverCheck,
						ValidationCode.Invalid.WithMessage("The value requested exceeds the value left to book."))
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
		public event ServiceEventHandler<ServiceEventArgs<TransactionOfferEntity>> OnTransactionOfferAnswered;
		public async Task<IResponse> AnswerTransactionOffer(IAsAccountEncryptableRequest<AnswerTransactionOfferParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				GetService<IEventfulManageExpirantsService>().DeleteExpirants<TransactionOfferEntity>();

				TransactionOfferEntity offer = Connection.GetSingle<TransactionOfferEntity>(request.Parameter.TransactionOfferId);

				IAccountEntity account = GetAccountEntity(request);

				Boolean authorizedCheck()
				{
					return account.HoldsManagerRightImplicitlyRecursively(Connection, offer);
				}
				Boolean answerCheck()
				{
					return request.Parameter.Answer != TransactionOfferAnswer.None;
				}
				async Task successAction()
				{
					if (account.Id == offer.Creator.Id)
					{
						Boolean creatorAnswerCheck()
						{
							return offer.CreatorConfirmation == TransactionOfferAnswer.None;
						}
						void creatorAnswerSuccessAction()
						{
							offer.CreatorConfirmation = request.Parameter.Answer;
						}

						await FirstCompound(creatorAnswerCheck,
							   ValidationField.Create(nameof(request.Parameter.Answer)),
							   ValidationCode.Invalid)
							.SetOnCriterionMet(creatorAnswerSuccessAction)
							.Evaluate(response);
					}
					else
					{
						Boolean recipientAnswerCheck()
						{
							return offer.RecipientAnswer == TransactionOfferAnswer.None;
						}
						void recipientAnswerSuccessAction()
						{
							offer.RecipientAnswer = request.Parameter.Answer;
						}

						await FirstCompound(recipientAnswerCheck,
							 ValidationField.Create(nameof(request.Parameter.Answer)),
							 ValidationCode.Invalid)
							.SetOnCriterionMet(recipientAnswerSuccessAction)
							.Evaluate(response);
					}
					Connection.Update(offer);
					Connection.SaveChanges();

					OnTransactionOfferAnswered.Invoke(Session, offer, offer.CloneAsT());

					UserEntity user = GetUserEntity(request);
					LogIfAccessingAsDelegate(user, "answered transaction offer");

					if (offer.RecipientAnswer == TransactionOfferAnswer.Accepted && offer.CreatorConfirmation == TransactionOfferAnswer.Accepted)
					{
						Expose(offer.SourceTransactionContract);

						OnSourceTransactionContractCreated.Invoke(Session, offer.SourceTransactionContract.Creditor, offer.SourceTransactionContract.CloneFor(offer.SourceTransactionContract.Creditor));
						OnSourceTransactionContractCreated.Invoke(Session, offer.SourceTransactionContract.Debtor, offer.SourceTransactionContract.CloneFor(offer.SourceTransactionContract.Debtor));

						LogIfAccessingAsDelegate(user, "created transaction source");

						Connection.Delete(offer);
						Connection.SaveChanges();
					}
					else if (offer.RecipientAnswer == TransactionOfferAnswer.Rejected || offer.CreatorConfirmation == TransactionOfferAnswer.Rejected)
					{
						Connection.Delete(offer.SourceTransactionContract);
						Connection.Delete(offer);
						Connection.SaveChanges();
					}
				}

				await FirstValidateAsAccount(request, response)
					.NextNullCheck(offer,
						ValidationField.Create(nameof(request.Parameter.TransactionOfferId)),
						ValidationCode.NotFound)
					.NextCompound(authorizedCheck,
						ValidationCode.Unauthorized)
					.InheritField()
					.NextCompound(answerCheck,
						ValidationField.Create(nameof(request.Parameter.Answer)),
						ValidationCode.Invalid.WithMessage("The answer can not be none."))
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}
		public async Task<IEncryptableResponse<SourceTransactionContractEntity>> GetSourceTransactionContract(IAsAccountEncryptableRequest<GetSourceTransactionContractParameter> request)
		{
			var response = new EncryptableResponse<SourceTransactionContractEntity>();

			async Task notNullRequest()
			{
				IAccountEntity account = GetAccountEntity(request);
				SourceTransactionContractEntity source = Connection.GetSingle<SourceTransactionContractEntity>(request.Parameter.SourceTransactionContractId);

				Boolean authorizedCheck()
				{
					return account.HoldsManagerRightImplicitlyRecursively(Connection, source);
				}

				void successAction()
				{
					response.Overwrite(source.CloneFor(account));

					UserEntity user = GetUserEntity(request);
					LogIfAccessingAsDelegate(user, "retrieved transaction source");
				}

				await FirstValidateAsAccount(request, response)
					.NextNullCheck(source,
						ValidationField.Create(nameof(request.Parameter.SourceTransactionContractId)),
						ValidationCode.NotFound)
					.NextCompound(authorizedCheck,
						ValidationCode.Unauthorized)
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
		public async Task<IEncryptableResponse<TargetTransactionContractEntity>> GetTargetTransactionContract(IAsAccountEncryptableRequest<GetTargetTransactionContractParameter> request)
		{
			var response = new EncryptableResponse<TargetTransactionContractEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				IAccountEntity account = GetAccountEntity(request);
				TargetTransactionContractEntity target = Connection.GetSingle<TargetTransactionContractEntity>(request.Parameter.TargetTransactionContractId);

				Boolean authorizedCheck()
				{
					return account.HoldsManagerRightImplicitlyRecursively(Connection, target);
				}

				void successAction()
				{
					response.Overwrite(target.CloneAsT());

					LogIfAccessingAsDelegate(user, "retrieved transaction target");
				}

				await FirstValidateAsAccount(request, response)
					.NextNullCheck(target,
						ValidationField.Create(nameof(request.Parameter.TargetTransactionContractId)),
						ValidationCode.NotFound)
					.NextCompound(authorizedCheck,
						ValidationCode.Unauthorized)
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
						ValidationField.Create(nameof(request.CurrencyId)),
						ValidationCode.NotFound.WithMessage("The currency requested could not be found."))
					.SetOnCriterionMet(setData)
					.Evaluate(response);
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

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
					.ThisValidatePagination(request, data)
					.SetOnCriterionMet(setData)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}
		public event ServiceEventHandler<ServiceEventArgs<CurrencyEntity>> OnCurrencyCreated;
		public async Task<IResponse> CreateCurrency(IRequest<CreateCurrencyParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				var name = request.Parameter.Name?.ToLower() ?? String.Empty;
				CurrencyEntity duplicate = Connection.GetFirst<CurrencyEntity>(c => c.Name.ToLower().Equals(name));

				Boolean roleCheck()
				{
					return Session.User.HoldsAdminRightImplicitly(Connection);
				}

				void successAction()
				{
					CurrencyEntity newCurrency = new(Session.User, request.Parameter.Name, request.Parameter.PluralName, request.Parameter.IngameTax);
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

				await FirstValidateAuthenticated()
					.NextCompound(roleCheck,
						ValidationField.Create(nameof(request)),
						ValidationCode.Unauthorized.WithMessage("You are not authorized to create currencies."))
					.NextNullCheck(duplicate,
						ValidationField.Create(nameof(request.Parameter.Name)),
						ValidationCode.Unauthorized.WithMessage("The currency requested has already been created."))
					.InvertCriterion()
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}
		public event ServiceEventHandler<ServiceEventArgs<CurrencyEntity>> OnCurrencyToggled;
		public async Task<IResponse> ToggleCurrency(IEncryptableRequest<ToggleCurrencyParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				CurrencyEntity currency = Connection.GetSingle<CurrencyEntity>(request.Parameter.CurrencyId);

				Boolean roleCheck()
				{
					return Session.User.HoldsAdminRightImplicitly(Connection);
				}
				void successAction()
				{
					if (currency.IsActive != request.Parameter.IsActive)
					{
						currency.IsActive = request.Parameter.IsActive;
						Connection.Update(currency);
						Connection.SaveChanges();

						OnCurrencyToggled.Invoke(Session, currency, currency.CloneAsT());
					}
				}

				await FirstValidateAuthenticated()
					.NextCompound(roleCheck,
						ValidationField.Create(nameof(request)),
						ValidationCode.Unauthorized.WithMessage("You are not authorized to toggle currencies."))
					.NextNullCheck(currency,
						ValidationField.Create(nameof(request.Parameter.CurrencyId)),
						ValidationCode.NotFound.WithMessage("The currency requested could not be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}
		public event ServiceEventHandler<ServiceEventArgs> OnCurrencyDeleted;
		public async Task<IResponse> DeleteCurrency(IEncryptableRequest<DeleteCurrencyParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				CurrencyEntity currency = Connection.GetSingle<CurrencyEntity>(request.Parameter.CurrencyId);

				Boolean roleCheck()
				{
					return Session.User.HoldsAdminRightImplicitly(Connection);
				}
				void successAction()
				{
					Connection.Delete(currency);
					Connection.Query<CurrencyBoolDictionaryEntity>()
						   .ToList()
						   .ForEach(d =>
							   {
								   d.TryRemoveValue(currency);
								   Connection.Update(d);
							   });
					Connection.SaveChanges();

					OnCurrencyDeleted.Invoke(currency);
				}

				await FirstValidateAuthenticated()
					.NextCompound(roleCheck,
						ValidationField.Create(nameof(request)),
						ValidationCode.Unauthorized.WithMessage("You are not not authorized to delete currencies."))
					.NextNullCheck(currency,
						ValidationField.Create(nameof(request.Parameter.CurrencyId)),
						ValidationCode.NotFound.WithMessage("The currency requested could not be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}
		public event ServiceEventHandler<ServiceEventArgs<SourceTransactionContractEntity>> OnEqualizationTransactionCreated;
		public async Task<IResponse> CreateEqualizationTransaction(IAsAccountRequest request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				void successAction()
				{
					UserEntity user = GetUserEntity(request);
					CitizenEntity citizen = GetCitizenEntity(request);
					VirtualAccountEntity account = GetAccountEntity<VirtualAccountEntity>(request);

					throw new NotImplementedException("Equalizing depends on middleman logic.");

					LogIfAccessingAsDelegate(user, "created equalizing transaction source");
				}

				await FirstValidateAsAccount(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}
		public event ServiceEventHandler<ServiceEventArgs<TransactionOfferEntity>> OnTransactionOfferManipulated;
		public async Task<IResponse> ManipulateTransactionOffer(IAsAccountEncryptableRequest<ManipulateTransactionOfferParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				var account = GetAccountEntity<VirtualAccountEntity>(request);
				TransactionOfferEntity offer = Connection.GetSingle<TransactionOfferEntity>(request.Parameter.TransactionOfferId);
				Lazy<DepositAccountReferenceEntity> forwardingReference = new(() => account.DepositReferences.SingleOrDefault(r => r.Id == request.Parameter.ForwardingAccountReferenceId));
				Lazy<ICollection<DepositAccountReferenceEntity>> depositReferences = new(() => request.Parameter.DepositAccountReferencesIds.Select(id => account.DepositReferences.SingleOrDefault(r => r.Id == id)).Distinct().ToList());

				Boolean depositReferencesCheck()
				{
					return depositReferences.Value.All(r => r != null);
				}
				void successAction()
				{
					ManipulateTransactionContractOffer(account,
																								 forwardingReference.Value,
																								 depositReferences.Value,
																								 offer);

					OnTransactionOfferManipulated.Invoke(Session, offer, offer.CloneAsT());

					var user = GetUserEntity(request);
					LogIfAccessingAsDelegate(user, "manipulated transaction offer");
				}

				await FirstValidateAsAccount(request, response)
					.NextEntityHoldsManagerImplicitlyRecursively(account, offer, Connection, ValidationField.Create(nameof(request.Parameter.TransactionOfferId)))
					.NextNullCheck(forwardingReference,
						ValidationField.Create(nameof(request.Parameter.ForwardingAccountReferenceId)),
						ValidationCode.NotFound.WithMessage("The forwarding reference requested could not be found."))
					.NextCompound(depositReferencesCheck,
						ValidationField.Create(nameof(request.Parameter.DepositAccountReferencesIds)),
						ValidationCode.PartiallyNotFound.WithMessage("Not all of the deposit references requested could be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}
		public async Task<IGetPaginatedEncryptableResponse<TransactionOfferEntity>> GetTransactionOffers(IAsAccountGetPaginatedRequest<GetTransactionOffersParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<TransactionOfferEntity>();

			async Task notNullRequest()
			{
				async Task successAction()
				{
					var account = GetAccountEntity(request);
					var data = Connection.Query<TransactionOfferEntity>()
						.Where(s => s.Creditor.Id == account.Id || s.Debtor.Id == account.Id)
						.FilterTransactions<TransactionOfferEntity, AccountEntityBase, AccountEntityBase, AccountEntityBase, AccountEntityBase>(request.Parameter.FilterProperty, request.Parameter.FilterComparator, request.Parameter.FilterValue)
						.OrderTransactions<TransactionOfferEntity, AccountEntityBase, AccountEntityBase, AccountEntityBase, AccountEntityBase>(request.Parameter.OrderByProperty, request.Parameter.OrderDescending);

					void setData()
					{
						response.LastPage = data.GetPageCount(request.PerPage) - 1;
						response.Data = data.Paginate(request.PerPage, request.Page).Select(a => a.CloneAsT()).ToList();

						UserEntity user = GetUserEntity(request);
						LogIfAccessingAsDelegate(user, "retrieved transaction offers");
					}

					await CachedCriterionChain.Cache.Get()
						.ThisValidatePagination(request, data)
						.SetOnCriterionMet(setData)
						.Evaluate(response);
				}

				await FirstValidateAsAccount(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}
		public async Task<IGetPaginatedEncryptableResponse<SourceTransactionContractEntity>> GetSourceTransactionContracts(IAsAccountGetPaginatedRequest<GetSourceTransactionContractsParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<SourceTransactionContractEntity>();

			async Task notNullRequest()
			{
				async Task successAction()
				{
					var account = GetAccountEntity(request);
					var data = Connection.Query<SourceTransactionContractEntity>()
						.Where(s => (s.Creditor.Id == account.Id || s.Debtor.Id == account.Id) && s.IsExposed)
						.FilterSourceTransactions(account, request.Parameter.FilterProperty, request.Parameter.FilterComparator, request.Parameter.FilterValue)
						.OrderSourceTransactions(account, request.Parameter.OrderByProperty, request.Parameter.OrderDescending);

					void setData()
					{
						response.LastPage = data.GetPageCount(request.PerPage) - 1;
						response.Data = data.Paginate(request.PerPage, request.Page).Select(a => a.CloneAsT()).ToList();

						UserEntity user = GetUserEntity(request);
						LogIfAccessingAsDelegate(user, "retrieved transaction sources");
					}

					await CachedCriterionChain.Cache.Get()
						.ThisValidatePagination(request, data)
						.SetOnCriterionMet(setData)
						.Evaluate(response);
				}

				await FirstValidateAsAccount(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}
		public async Task<IGetPaginatedEncryptableResponse<TargetTransactionContractEntity>> GetTargetTransactionContracts(IAsAccountGetPaginatedRequest<GetTargetTransactionContractsParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<TargetTransactionContractEntity>();

			async Task notNullRequest()
			{
				async Task successAction()
				{
					var account = GetAccountEntity(request);

					var data = Connection.Query<TargetTransactionContractEntity>()
						.Where(s => (s.Creditor.Id == account.Id || s.Debtor.Id == account.Id) && s.IsExposed)
						.FilterTargetTransactions(account, request.Parameter.FilterProperty, request.Parameter.FilterComparator, request.Parameter.FilterValue)
						.OrderTargetTransactions(account, request.Parameter.OrderByProperty, request.Parameter.OrderDescending);

					void setData()
					{
						response.LastPage = data.GetPageCount(request.PerPage) - 1;
						response.Data = data.Paginate(request.PerPage, request.Page).Select(a => a.CloneAsT()).ToList();

						UserEntity user = GetUserEntity(request);
						LogIfAccessingAsDelegate(user, "retrieved transaction targets");
					}

					await CachedCriterionChain.Cache.Get()
						.ThisValidatePagination(request, data)
						.SetOnCriterionMet(setData)
						.Evaluate(response);
				}

				await FirstValidateAsAccount(request, response)
						.SetOnCriterionMet(successAction)
						.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}

		public Boolean SuperficialPartnersRelationshipIsReal(IAccountEntity partnerA, IAccountEntity partnerB)
		{
			return RealTransactionPartnersRelationships.Contains(GetSuperficialRelationship(partnerA, partnerB));
		}
		public TransactionPartnersRelationship GetSuperficialRelationship(IAccountEntity creditor, IAccountEntity debtor)
		{
			if (creditor.Id == debtor.Id && creditor.Is<VirtualAccountEntity>(Connection))
			{
				return TransactionPartnersRelationship.Equalizing;
			}
			if (creditor.Is<VirtualAccountEntity>(Connection))
			{
				if (debtor.Is<VirtualAccountEntity>(Connection))
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
				if (debtor is VirtualAccountEntity)
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
					GetService<IEventfulAccountService>().UpdateDepositBalance(account.As<VirtualAccountEntity>(Connection), referencedAccount, value);
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
			List<TargetTransactionContractEntity> targetTransactionContracts = new() { };

			IEnumerable<DepositAccountReferenceEntity> getActiveDeposits(VirtualAccountEntity account)
			{
				return account.DepositReferences.Where(r => r.IsActive && r.Currency.Id == currency.Id);
			};

			RealAccountEntity getForwardingAccount(VirtualAccountEntity account)
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
				Lazy<VirtualAccountEntity> virtualCreditor = new(() => Connection.GetFirst<VirtualAccountEntity>(creditor.Id));
				Lazy<VirtualAccountSettingsEntity> virtualCreditorSettings = new(() => GetSettings<VirtualAccountSettingsEntity>(virtualCreditor.Value));

				Lazy<VirtualAccountEntity> virtualDebtor = new(() => Connection.GetFirst<VirtualAccountEntity>(debtor.Id));
				Lazy<VirtualAccountSettingsEntity> virtualDebtorSettings = new(() => GetSettings<VirtualAccountSettingsEntity>(virtualDebtor.Value));

				Lazy<RealAccountEntity> realCreditor = new(() => Connection.GetFirst<RealAccountEntity>(creditor.Id));
				Lazy<RealAccountSettingsEntity> realCreditorSettings = new(() => GetSettings<RealAccountSettingsEntity>(realCreditor.Value));

				Lazy<RealAccountEntity> realDebtor = new(() => Connection.GetFirst<RealAccountEntity>(debtor.Id));
				Lazy<RealAccountSettingsEntity> realDebtorSettings = new(() => GetSettings<RealAccountSettingsEntity>(realDebtor.Value));

				Lazy<CBData.Abstractions.IAccountSettingsEntity> recipientSettings = new(() => GetSettings<IAccountSettingsEntity>(recipient));

				additionalUntilDue += recipientSettings.Value.MinimumContractLifeSpan;

				List<(RealAccountEntity referencedAccount, Decimal cut)> getDepositCuts(VirtualAccountEntity account, Decimal value)
				{
					List<(RealAccountEntity referencedAccount, Decimal cut)> retVal = new() { };
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
						retVal.Add((referencedAccount, cut: val));
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
							var creditorDepositCuts = getDepositCuts(virtualCreditor.Value, net);

							foreach (var (referencedAccount, cut) in creditorDepositCuts)
							{
								targetTransactionContracts.Add(new TargetTransactionContractEntity(creator,
																								   recipient,
																								   referencedAccount,
																								   creditorForwardingAccount,
																								   CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																								   currency.IngameTax,
																								   currency,
																								   cut,
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
							var debtorDepositCuts = getDepositCuts(virtualDebtor.Value, targetTransactionContracts.Sum(c => c.Gross));
							foreach (var (referencedAccount, cut) in debtorDepositCuts)
							{
								targetTransactionContracts.Add(new TargetTransactionContractEntity(creator,
																							   recipient,
																								   debtorForwardingAccount,
																								   referencedAccount,
																								   CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																								   currency.IngameTax,
																								   currency,
																								   cut,
																								   TransactionPartnersRelationship.DepositToForward,
																								   virtualDebtorSettings.Value.DepositForwardLifeSpan));
							}
							break;
						}
					case TransactionPartnersRelationship.VirtualToVirtual:
						{
							RealAccountEntity debtorForwardingAccount = getForwardingAccount(virtualDebtor.Value);
							RealAccountEntity creditorForwardingAccount = getForwardingAccount(virtualCreditor.Value);

							var creditorDepositCuts = getDepositCuts(virtualCreditor.Value, net);
							foreach (var (referencedAccount, cut) in creditorDepositCuts)
							{
								targetTransactionContracts.Add(new TargetTransactionContractEntity(creator,
																							   recipient,
																								   referencedAccount,
																								   creditorForwardingAccount,
																								   CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																								   currency.IngameTax,
																								   currency,
																								   cut,
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

							var debtorDepositCuts = getDepositCuts(virtualDebtor.Value, targetTransactionContracts.Where(t => t.Relationship == TransactionPartnersRelationship.ForwardToForward).Sum(c => c.Gross));
							foreach (var (referencedAccount, cut) in debtorDepositCuts)
							{
								targetTransactionContracts.Add(new TargetTransactionContractEntity(creator,
																								   recipient,
																								   debtorForwardingAccount,
																								   referencedAccount,
																								   CBCommon.Settings.CitizenBank.DefaultGeneratedMessage,
																								   currency.IngameTax,
																								   currency,
																								   cut,
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
		public void ManipulateTransactionContractOffer(VirtualAccountEntity manipulator,
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
				List<Tuple<RealAccountEntity, Decimal>> depositCuts = new() { };
				Decimal absoluteLimit = depositAccountReferences.Sum(d => d.CalculatedAbsoluteLimit);
				foreach (DepositAccountReferenceEntity d in depositAccountReferences)
				{
					depositCuts.Add(new Tuple<RealAccountEntity, Decimal>(Connection.GetFirst<RealAccountEntity>(d.ReferencedAccount.Id), (d.CalculatedAbsoluteLimit / absoluteLimit) * value));
				}
				return depositCuts;
			};

			List<Tuple<RealAccountEntity, Decimal>> getOtherCuts(VirtualAccountEntity account, Decimal value)
			{
				List<Tuple<RealAccountEntity, Decimal>> depositCuts = new() { };
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

			Lazy<VirtualAccountSettingsEntity> manipulatorSettings = new(() => GetSettings<VirtualAccountSettingsEntity>(manipulator));

			RealAccountEntity manipulatorForwardingAccount = Connection.GetFirst<RealAccountEntity>(forwardingAccountReference.ReferencedAccount.Id);
			List<TargetTransactionContractEntity> newTargets = new() { };

			if (manipulatorIsCreditor)
			{
				if (offer.Debtor.Is<VirtualAccountEntity>(out VirtualAccountEntity virtualDebtor, Connection))
				{
					Lazy<VirtualAccountSettingsEntity> debtorSettings = new(() => GetSettings<VirtualAccountSettingsEntity>(virtualDebtor));

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
					TargetTransactionContractEntity newFTFContract = new(manipulator,
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
					TargetTransactionContractEntity newRTFContract = new(manipulator,
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
				if (offer.Creditor.Is<VirtualAccountEntity>(out VirtualAccountEntity virtualCreditor, Connection))
				{
					Lazy<VirtualAccountSettingsEntity> creditorSettings = new(() => GetSettings<VirtualAccountSettingsEntity>(virtualCreditor));
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

					TargetTransactionContractEntity newFTFContract = new(manipulator,
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
					TargetTransactionContractEntity newFTRContract = new(manipulator,
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
			SourceTransactionContractEntity newSource = new(manipulator,
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
							.As<VirtualAccountEntity>(Connection)
							.DepositReferences
							.Single(r => r.ReferencedAccount.Equals(target.Debtor));
						return bookingValue <= reference.AbsoluteBalance;
					}
				case TransactionPartnersRelationship.ForwardToDeposit:
					{
						DepositAccountReferenceEntity reference = source
							.Creditor
							.As<VirtualAccountEntity>(Connection)
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
			var now = TimeManager.Now;

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
						List<Guid> recipients = new()
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
