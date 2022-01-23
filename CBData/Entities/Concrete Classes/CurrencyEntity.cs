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
	public class CurrencyEntity : PluralNamedEntityBase, IHasCreator<UserEntity>
	{
		public CurrencyEntity(UserEntity creator, String name, String pluralName, Decimal ingameTax) : base(name, pluralName)
		{
			IngameTax = ingameTax;
			Creator = creator;
		}

		public CurrencyEntity() { }
		protected CurrencyEntity(CurrencyEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			IngameTax = from.IngameTax;
			IsActive = from.IsActive;
			Creator = from.Creator.CloneAsT(circularReferenceHelperDictionary);
		}

		public virtual Boolean IsActive { get; set; }
		public virtual Decimal IngameTax { get; set; }
		public virtual UserEntity Creator { get; set; }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new CurrencyEntity(this, circularReferenceHelperDictionary);
		}

		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			await Creator.SafeEncrypt(encryptor);
			await base.EncryptSelf(encryptor);
		}
		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			await Creator.SafeDecrypt(decryptor);
			await base.DecryptSelf(decryptor);
		}
	}
}
