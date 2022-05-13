
using CBData.Abstractions;
using CBData.Entities;



using PBApplication.Events;
using PBApplication.Services.Abstractions;

using System;
using System.Collections.Generic;
using static CBCommon.Enums.CitizenBankEnums;

namespace CBApplication.Services.Abstractions
{
	public interface IEventfulTransactionService : ITransactionService, IEventfulService
	{
		//Payload : new offer
		//Recipients : creditor, debtor
		event ServiceEventHandler<ServiceEventArgs<TransactionOfferEntity>> OnTransactionOfferCreated;
		//Payload : new booking
		//Recipients : affected target
		event ServiceEventHandler<ServiceEventArgs<BookingEntity>> OnBookingCreated;
		//Payload : new offer
		//Recipients : affected offer
		event ServiceEventHandler<ServiceEventArgs<TransactionOfferEntity>> OnTransactionOfferAnswered;
		//Payload : new source
		//Recipients : creditor, debtor
		event ServiceEventHandler<ServiceEventArgs<SourceTransactionContractEntity>> OnSourceTransactionContractCreated;
		//Payload : new offer
		//Recipients : affected offer
		event ServiceEventHandler<ServiceEventArgs<TransactionOfferEntity>> OnTransactionOfferManipulated;
		//Payload : new currency
		//Recipients : logged in sessions, affected dictionaries
		event ServiceEventHandler<ServiceEventArgs<CurrencyEntity>> OnCurrencyCreated;
		//Payload : new currency
		//Recipients : affected currency
		event ServiceEventHandler<ServiceEventArgs<CurrencyEntity>> OnCurrencyToggled;
		//Recipients : affected currency
		event ServiceEventHandler<ServiceEventArgs> OnCurrencyDeleted;
		//Payload : new source
		//Recipients : creditor
		event ServiceEventHandler<ServiceEventArgs<SourceTransactionContractEntity>> OnEqualizationTransactionCreated;
		//Payload : new target
		//Recipients : creditor, debtor, source
		event ServiceEventHandler<ServiceEventArgs<TargetTransactionContractEntity>> OnTargetExposed;

		Boolean SuperficialPartnersRelationshipIsReal(IAccountEntity partnerA, IAccountEntity partnerB);
		void UpdateDepositBalanceIfBooked(SourceTransactionContractEntity source, TargetTransactionContractEntity target);
		TransactionPartnersRelationship GetSuperficialRelationship(IAccountEntity partnerA, IAccountEntity partnerB);
		SourceTransactionContractEntity CreateSourceTransactionContract<TCreditor, TDebtor, TCreator, TRecipient>(TCreditor creditor,
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
			where TRecipient : AccountEntityBase;
		void ManipulateTransactionContractOffer(VirtualAccountEntity manipulator,
												DepositAccountReferenceEntity forwardingAccountReference,
												ICollection<DepositAccountReferenceEntity> depositAccountReferences,
												TransactionOfferEntity offer);
		Boolean ValidateBookingValue(SourceTransactionContractEntity source, TargetTransactionContractEntity target, IAccountEntity bookingAccount, Decimal bookingValue);
		SourceTransactionContractEntity CloneAsAccount(SourceTransactionContractEntity source, IAccountEntity account);
		ICollection<CurrencyEntity> GetCurrencies();
		void Expose(SourceTransactionContractEntity source);
	}
}
