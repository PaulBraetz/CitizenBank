using CBData.Abstractions;
using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;
using PBData.Entities;
using PBData.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBData.Entities
{
	public abstract class AccountEntityBase : NamedEntityBase, IAccountEntity
	{
		protected AccountEntityBase() { }
		protected AccountEntityBase(AccountEntityBase from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			CreditScore = from.CreditScore.CloneAsT(circularReferenceHelperDictionary);
			Creator = from.Creator.CloneAsT(circularReferenceHelperDictionary);
			Tags = from.Tags.CloneAsT(circularReferenceHelperDictionary).ToList();
			PriorityTags = from.PriorityTags.CloneAsT(circularReferenceHelperDictionary).ToList();
		}
		protected AccountEntityBase(CitizenEntity creator, String name, CreditScoreEntity creditScore) : base(name)
		{
			CreditScore = creditScore;
			Creator = creator;
			Tags = new List<TagEntity>();
			PriorityTags = new List<TagEntity>();
		}

		public virtual CreditScoreEntity CreditScore { get; set; }
		public virtual CitizenEntity Creator { get; set; }
		public virtual ICollection<TagEntity> Tags { get; set; }
		public virtual ICollection<TagEntity> PriorityTags { get; set; }

		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			await Task.WhenAll(
				CreditScore.SafeEncrypt(encryptor),
				Creator.SafeEncrypt(encryptor),
				Tags.SafeEncrypt(encryptor),
				PriorityTags.SafeEncrypt(encryptor));
			await base.EncryptSelf(encryptor);
		}
		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			await Task.WhenAll(
				CreditScore.SafeDecrypt(decryptor),
				Creator.SafeDecrypt(decryptor),
				Tags.SafeDecrypt(decryptor),
				PriorityTags.SafeDecrypt(decryptor));
			await base.DecryptSelf(decryptor);
		}

		public void AttachTo(UserSessionEntity session)
		{
			return;
		}

		public void DetachFrom(UserSessionEntity session)
		{
			return;
		}
	}
}
