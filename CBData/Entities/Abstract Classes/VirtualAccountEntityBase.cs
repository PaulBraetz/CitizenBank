using CBData.Abstractions;
using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;
using PBData.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBData.Entities
{
	public abstract class VirtualAccountEntityBase : AccountEntityBase, IVirtualAccountEntity
	{
		protected VirtualAccountEntityBase()
		{
		}
		protected VirtualAccountEntityBase(VirtualAccountEntityBase from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			DepositReferences = from.DepositReferences?.CloneAsT(circularReferenceHelperDictionary).ToList() ?? new List<DepositAccountReferenceEntity>();
		}

		protected VirtualAccountEntityBase(CitizenEntity creator, String name, CreditScoreEntity creditScore) : base(creator, name, creditScore)
		{
		}

		public virtual ICollection<DepositAccountReferenceEntity> DepositReferences { get; set; }

		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			await DepositReferences.SafeEncrypt(encryptor);
			await base.EncryptSelf(encryptor);
		}

		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			await DepositReferences.SafeDecrypt(decryptor);
			await base.DecryptSelf(decryptor);
		}
	}
}
