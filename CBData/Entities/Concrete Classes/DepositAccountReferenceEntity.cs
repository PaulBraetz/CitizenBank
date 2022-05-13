using CBData.Abstractions;
using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;
using PBData.Abstractions;
using PBData.Entities;
using PBData.Extensions;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CBData.Entities
{
	public class DepositAccountReferenceEntity : EntityBase, IActivable, IHasCurrency<CurrencyEntity>
	{
		public DepositAccountReferenceEntity(RealAccountEntity depositAccount, CurrencyEntity currency)
		{
			ReferencedAccount = depositAccount;
			Currency = currency;
		}

		public DepositAccountReferenceEntity() { }
		protected DepositAccountReferenceEntity(DepositAccountReferenceEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			AbsoluteBalance = from.AbsoluteBalance;
			RelativeBalance = from.RelativeBalance;
			AbsoluteLimit = from.AbsoluteLimit;
			RelativeLimit = from.RelativeLimit;
			CalculatedAbsoluteLimit = from.CalculatedAbsoluteLimit;
			CalculatedRelativeLimit = from.CalculatedRelativeLimit;
			Saturation = from.Saturation;
			AbsoluteBalance = from.AbsoluteBalance;
			Currency = from.Currency.CloneAsT(circularReferenceHelperDictionary);
			IsActive = from.IsActive;
			ReferencedAccount = from.ReferencedAccount.CloneAsT(circularReferenceHelperDictionary);
		}

		public virtual Decimal AbsoluteBalance { get; set; }
		public virtual Decimal RelativeBalance { get; set; }
		public virtual Decimal AbsoluteLimit { get; set; }
		public virtual Decimal RelativeLimit { get; set; }
		public virtual Decimal CalculatedAbsoluteLimit { get; set; }
		public virtual Decimal CalculatedRelativeLimit { get; set; }
		public virtual Decimal Saturation { get; set; }
		public virtual CurrencyEntity Currency { get; set; }
		public virtual Boolean IsActive { get; set; }
		public virtual Boolean UseAsForwarding { get; set; }
		public virtual RealAccountEntity ReferencedAccount { get; set; }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new DepositAccountReferenceEntity(this, circularReferenceHelperDictionary);
		}
		public virtual DepositAccountReferenceEntity AdvancedClone()
		{
			return this.CloneAsT(new Dictionary<Guid, Object>());
		}
		public virtual DepositAccountReferenceEntity BasicClone()
		{
			var retVal = this.CloneAsT(new Dictionary<Guid, Object>());
			retVal.RelativeBalance = 0;
			retVal.RelativeLimit = 0;
			retVal.CalculatedAbsoluteLimit = 0;
			retVal.CalculatedRelativeLimit = 0;
			retVal.Saturation = 0;
			return retVal;
		}

		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			await Task.WhenAll(
				Currency.SafeEncrypt(encryptor),
				ReferencedAccount.SafeEncrypt(encryptor));
			await base.EncryptSelf(encryptor);
		}
		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			await Task.WhenAll(
				Currency.SafeDecrypt(decryptor),
				ReferencedAccount.SafeDecrypt(decryptor));
			await base.DecryptSelf(decryptor);
		}
	}
}
