
using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;
using PBData.Abstractions;
using PBData.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBData.Entities
{
	public class VirtualAccountEntity : VirtualAccountEntityBase, IHasAdmins<CitizenEntity>, IHasOwner<CitizenEntity>
	{
		public VirtualAccountEntity(CitizenEntity creator, String name, CreditScoreEntity creditScore) : base(creator, name, creditScore)
		{
			Admins = new List<CitizenEntity> { };
			Owner = creator;
		}

		public VirtualAccountEntity()
		{
		}
		protected VirtualAccountEntity(VirtualAccountEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			Owner = from.Owner.CloneAsT(circularReferenceHelperDictionary);
			Admins = from.Admins.CloneAsT(circularReferenceHelperDictionary).ToList();
		}

		public virtual CitizenEntity Owner { get; set; }
		public virtual ICollection<CitizenEntity> Admins { get; set; }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new VirtualAccountEntity(this, circularReferenceHelperDictionary);
		}

		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			await Task.WhenAll(
				Owner.SafeEncrypt(encryptor),
				Admins.SafeEncrypt(encryptor));
			await base.EncryptSelf(encryptor);
		}
		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			await Task.WhenAll(
				Owner.SafeDecrypt(decryptor),
				Admins.SafeDecrypt(decryptor));
			await base.DecryptSelf(decryptor);
		}
	}
}
