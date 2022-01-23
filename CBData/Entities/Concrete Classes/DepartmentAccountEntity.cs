using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;
using PBData.Extensions;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CBData.Entities
{
	public class DepartmentAccountEntity : VirtualAccountEntityBase
	{
		public DepartmentAccountEntity(CitizenEntity creator, String name, CreditScoreEntity creditScore, DepartmentEntityBase department) : base(creator, name, creditScore)
		{
			Department = department;
		}

		public DepartmentAccountEntity()
		{
		}
		protected DepartmentAccountEntity(DepartmentAccountEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			Department = from.Department.CloneAsT(circularReferenceHelperDictionary);
		}

		public virtual DepartmentEntityBase Department { get; set; }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new DepartmentAccountEntity(this, circularReferenceHelperDictionary);
		}

		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			await Department.SafeEncrypt(encryptor);
			await base.EncryptSelf(encryptor);
		}
		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			await Department.SafeDecrypt(decryptor);
			await base.DecryptSelf(decryptor);
		}
	}
}
