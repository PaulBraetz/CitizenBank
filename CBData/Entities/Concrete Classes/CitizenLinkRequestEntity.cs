using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;
using PBData.Abstractions;
using PBData.Entities;
using PBData.Extensions;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PBCommon.Enums;

namespace CBData.Entities
{
	public class CitizenLinkRequestEntity : ExpiringEntityBase
	{
		public CitizenLinkRequestEntity(UserEntity owner,
										 CitizenEntity citizen,
										 String verificationCode) : base(TimeSpan.FromDays(28), true, false)
		{
			Owner = owner;
			CitizenName = citizen.Name;
			VerificationCode = verificationCode;
		}

		public CitizenLinkRequestEntity()
		{
		}
		protected CitizenLinkRequestEntity(CitizenLinkRequestEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			Owner = from.Owner.CloneAsT(circularReferenceHelperDictionary);
			CitizenName = from.CitizenName;
			VerificationCode = from.VerificationCode;
		}

		public virtual UserEntity Owner { get; set; }
		public virtual String CitizenName { get; set; }
		public virtual String VerificationCode { get; set; }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new CitizenLinkRequestEntity(this, circularReferenceHelperDictionary);
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
