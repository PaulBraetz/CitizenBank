using PBCommon.Encryption.Abstractions;
using PBData.Abstractions;
using PBData.Extensions;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CBData.Entities
{

	public class RealAccountEntity : AccountEntityBase, IHasOwner<CitizenEntity>
	{
		public RealAccountEntity(CitizenEntity creator, CreditScoreEntity creditScore) : base(creator, creator.Name, creditScore)
		{
			Owner = creator;
		}

		public RealAccountEntity()
		{
		}
		protected RealAccountEntity(RealAccountEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			Owner = from.Owner.CloneAsT(circularReferenceHelperDictionary);
		}

		public virtual CitizenEntity Owner { get; set; }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new RealAccountEntity(this, circularReferenceHelperDictionary);
		}

		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			await Owner.Encrypt(encryptor);
			await base.EncryptSelf(encryptor);
		}
		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			await Owner.Decrypt(decryptor);
			await base.DecryptSelf(decryptor);
		}
	}
}
