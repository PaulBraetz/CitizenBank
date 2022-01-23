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

	public class CitizenEntity : NamedEntityBase, IHasOwner<UserEntity>
	{
		public CitizenEntity(String name) : base(name) { }

		public CitizenEntity() { }
		protected CitizenEntity(CitizenEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			Owner = from.Owner.CloneAsT(circularReferenceHelperDictionary);
		}

		public virtual UserEntity Owner { get; set; }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new CitizenEntity(this, circularReferenceHelperDictionary);
		}
		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			await Owner.SafeEncrypt(encryptor);
			await base.EncryptSelf(encryptor);
		}
		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			await Owner.SafeDecrypt(decryptor);
			await base.DecryptSelf(decryptor);
		}
	}
}
