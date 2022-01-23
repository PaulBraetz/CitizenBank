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

	public abstract class DepartmentEntityBase : NamedEntityBase, IDepartmentEntity
	{
		protected DepartmentEntityBase() { }
		protected DepartmentEntityBase(DepartmentEntityBase from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			SubDepartments = from.SubDepartments.CloneAsT(circularReferenceHelperDictionary).ToList();
			Tags = from.Tags.CloneAsT(circularReferenceHelperDictionary).ToList();
			PriorityTags = from.PriorityTags.CloneAsT(circularReferenceHelperDictionary).ToList();
			Admins = from.Admins.CloneAsT(circularReferenceHelperDictionary).ToList();
			Members = from.Members.CloneAsT(circularReferenceHelperDictionary).ToList();
			Creator = from.Creator.CloneAsT(circularReferenceHelperDictionary);
		}
		protected DepartmentEntityBase(String name) : base(name)
		{
			SubDepartments = new List<SubDepartmentEntity>();
			Tags = new List<TagEntity>();
			PriorityTags = new List<TagEntity>();
			Admins = new List<CitizenEntity>();
			Members = new List<AccountEntityBase>();
		}
		protected DepartmentEntityBase(CitizenEntity creator, String name) : this(name)
		{
			Creator = creator;
		}

		public virtual ICollection<SubDepartmentEntity> SubDepartments { get; set; }
		public virtual ICollection<TagEntity> Tags { get; set; }
		public virtual ICollection<TagEntity> PriorityTags { get; set; }
		public virtual ICollection<CitizenEntity> Admins { get; set; }
		public virtual ICollection<AccountEntityBase> Members { get; set; }
		public virtual CitizenEntity Creator { get; set; }

		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			await Task.WhenAll(
				SubDepartments.SafeEncrypt(encryptor),
				Tags.SafeEncrypt(encryptor),
				PriorityTags.SafeEncrypt(encryptor),
				Admins.SafeEncrypt(encryptor),
				Members.SafeEncrypt(encryptor),
				Creator.SafeEncrypt(encryptor));
			await base.EncryptSelf(encryptor);
		}
		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			await Task.WhenAll(
				SubDepartments.SafeDecrypt(decryptor),
				Tags.SafeDecrypt(decryptor),
				PriorityTags.SafeDecrypt(decryptor),
				Admins.SafeDecrypt(decryptor),
				Members.SafeDecrypt(decryptor),
				Creator.SafeDecrypt(decryptor));
			await base.DecryptSelf(decryptor);
		}
	}
}
