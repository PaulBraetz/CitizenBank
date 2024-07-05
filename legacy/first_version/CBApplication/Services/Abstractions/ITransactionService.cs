using CBApplication.Requests.Abstractions;
using CBData.Entities;
using PBApplication.Requests.Abstractions;
using PBApplication.Responses.Abstractions;
using PBApplication.Services.Abstractions;
using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CBCommon.Enums.CitizenBankEnums;

namespace CBApplication.Services.Abstractions
{
	public interface ITransactionService : IService
	{
		sealed class CreateTransactionOfferParameter : EncryptableBase<Guid>
		{
			public Guid RecipientId { get; set; }
			public Guid DebtorId { get; set; }
			public Guid CreditorId { get; set; }
			public Guid CurrencyId { get; set; }
			public Int32 AdditionalDaysUntilDue { get; set; }
			public Decimal Value { get; set; }
			public String Usage { get; set; }
			public IEnumerable<String> TagsTexts { get; set; }

			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				RecipientId = await decryptor.Decrypt(RecipientId);
				DebtorId = await decryptor.Decrypt(DebtorId);
				CreditorId = await decryptor.Decrypt(CreditorId);
				CurrencyId = await decryptor.Decrypt(CurrencyId);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				RecipientId = await encryptor.Encrypt(RecipientId);
				DebtorId = await encryptor.Encrypt(DebtorId);
				CreditorId = await encryptor.Encrypt(CreditorId);
				CurrencyId = await encryptor.Encrypt(CurrencyId);
			}
		}
		Task<IEncryptableResponse<SourceTransactionContractEntity>> GetTransactionSourcePreview(IAsAccountEncryptableRequest<CreateTransactionOfferParameter> request);
		Task<IResponse> CreateTransactionOffer(IAsAccountEncryptableRequest<CreateTransactionOfferParameter> request);

		sealed class CreateBookingParameter : EncryptableBase<Guid>
		{
			public Guid TargetTransactionId { get; set; }
			public Decimal Value { get; set; }
			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				TargetTransactionId = await decryptor.Decrypt(TargetTransactionId);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				TargetTransactionId = await encryptor.Encrypt(TargetTransactionId);
			}
		}
		Task<IResponse> CreateBooking(IAsAccountEncryptableRequest<CreateBookingParameter> request);

		sealed class AnswerTransactionOfferParameter : EncryptableBase<Guid>
		{
			public Guid TransactionOfferId { get; set; }
			public TransactionOfferAnswer Answer { get; set; }
			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				TransactionOfferId = await decryptor.Decrypt(TransactionOfferId);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				TransactionOfferId = await encryptor.Encrypt(TransactionOfferId);
			}
		}
		Task<IResponse> AnswerTransactionOffer(IAsAccountEncryptableRequest<AnswerTransactionOfferParameter> request);

		abstract class GetTransactionsParameterBase
		{
			public TransactionContractFilterComparator FilterComparator { get; set; }
			public String FilterValue { get; set; }
			public Boolean OrderDescending { get; set; }
		}
		sealed class GetTransactionOffersParameter : GetTransactionsParameterBase
		{
			public SimpleTransactionContractProperty OrderByProperty { get; set; }
			public SimpleTransactionContractProperty FilterProperty { get; set; }
		}
		Task<IGetPaginatedEncryptableResponse<TransactionOfferEntity>> GetTransactionOffers(IAsAccountGetPaginatedRequest<GetTransactionOffersParameter> request);

		abstract class GetTransactionContractsParameterBase : GetTransactionsParameterBase
		{
			public AdvancedTransactionContractProperty OrderByProperty { get; set; }
			public AdvancedTransactionContractProperty FilterProperty { get; set; }
		}
		sealed class GetSourceTransactionContractsParameter : GetTransactionContractsParameterBase
		{
		}
		Task<IGetPaginatedEncryptableResponse<SourceTransactionContractEntity>> GetSourceTransactionContracts(IAsAccountGetPaginatedRequest<GetSourceTransactionContractsParameter> request);

		sealed class GetSourceTransactionContractParameter : EncryptableBase<Guid>
		{
			public Guid SourceTransactionContractId { get; set; }
			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				SourceTransactionContractId = await decryptor.Decrypt(SourceTransactionContractId);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				SourceTransactionContractId = await encryptor.Encrypt(SourceTransactionContractId);
			}
		}
		Task<IEncryptableResponse<SourceTransactionContractEntity>> GetSourceTransactionContract(IAsAccountEncryptableRequest<GetSourceTransactionContractParameter> request);

		sealed class GetTargetTransactionContractsParameter : GetTransactionContractsParameterBase
		{ }
		Task<IGetPaginatedEncryptableResponse<TargetTransactionContractEntity>> GetTargetTransactionContracts(IAsAccountGetPaginatedRequest<GetTargetTransactionContractsParameter> request);

		sealed class GetTargetTransactionContractParameter : EncryptableBase<Guid>
		{
			public Guid TargetTransactionContractId { get; set; }
			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				TargetTransactionContractId = await decryptor.Decrypt(TargetTransactionContractId);
			}
			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				TargetTransactionContractId = await encryptor.Encrypt(TargetTransactionContractId);
			}
		}
		Task<IEncryptableResponse<TargetTransactionContractEntity>> GetTargetTransactionContract(IAsAccountEncryptableRequest<GetTargetTransactionContractParameter> request);

		sealed class ManipulateTransactionOfferParameter : EncryptableBase<Guid>
		{
			public Guid TransactionOfferId { get; set; }
			public Guid ForwardingAccountReferenceId { get; set; }
			public ICollection<Guid> DepositAccountReferencesIds { get; set; }
			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				TransactionOfferId = await decryptor.Decrypt(TransactionOfferId);
				ForwardingAccountReferenceId = await decryptor.Decrypt(ForwardingAccountReferenceId);
				DepositAccountReferencesIds = await decryptor.Decrypt(DepositAccountReferencesIds);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				TransactionOfferId = await encryptor.Encrypt(TransactionOfferId);
				ForwardingAccountReferenceId = await encryptor.Encrypt(ForwardingAccountReferenceId);
				DepositAccountReferencesIds = await encryptor.Encrypt(DepositAccountReferencesIds);
			}
		}
		Task<IResponse> ManipulateTransactionOffer(IAsAccountEncryptableRequest<ManipulateTransactionOfferParameter> request);

		sealed class GetCurrencyParameter : EncryptableBase<Guid>
		{
			public Guid CurrencyId { get; set; }
			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				CurrencyId = await decryptor.Decrypt(CurrencyId);
			}
			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				CurrencyId = await encryptor.Encrypt(CurrencyId);
			}
		}
		Task<IEncryptableResponse<CurrencyEntity>> GetCurrency(GetCurrencyParameter request);

		sealed class GetCurrenciesParameter
		{
		}
		Task<IGetPaginatedEncryptableResponse<CurrencyEntity>> GetCurrencies(IGetPaginatedRequest<GetCurrenciesParameter> request);

		sealed class CreateCurrencyParameter
		{
			public String Name { get; set; }
			public String PluralName { get; set; }
			public Decimal IngameTax { get; set; }
		}
		Task<IResponse> CreateCurrency(IRequest<CreateCurrencyParameter> request);

		sealed class ToggleCurrencyParameter : EncryptableBase<Guid>
		{
			public Guid CurrencyId { get; set; }
			public Boolean IsActive { get; set; }
			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				CurrencyId = await decryptor.Decrypt(CurrencyId);
			}
			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				CurrencyId = await encryptor.Encrypt(CurrencyId);
			}
		}
		Task<IResponse> ToggleCurrency(IEncryptableRequest<ToggleCurrencyParameter> request);

		sealed class DeleteCurrencyParameter : EncryptableBase<Guid>
		{
			public Guid CurrencyId { get; set; }
			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				CurrencyId = await decryptor.Decrypt(CurrencyId);
			}
			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				CurrencyId = await encryptor.Encrypt(CurrencyId);
			}
		}
		Task<IResponse> DeleteCurrency(IEncryptableRequest<DeleteCurrencyParameter> request);

		Task<IResponse> CreateEqualizationTransaction(IAsAccountRequest request);
	}
}
