namespace CBData.Entities
{
    public class CitizenLinkRequestEntity : ExpiringEntityBase
	{
		public CitizenLinkRequestEntity(UserEntity owner,
										 CitizenEntity citizen,
										 String verificationCode) : base(TimeSpan.FromDays(28), true, false)
		{
			User = owner;
			Citizen = citizen;
			VerificationCode = verificationCode;
		}

		public CitizenLinkRequestEntity()
		{
		}
		protected CitizenLinkRequestEntity(CitizenLinkRequestEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			User = from.User.CloneAsT(circularReferenceHelperDictionary);
			Citizen = from.Citizen.CloneAsT(circularReferenceHelperDictionary);
			VerificationCode = from.VerificationCode;
		}

		public virtual UserEntity User { get; set; }
		public virtual CitizenEntity Citizen { get; set; }
		public virtual String VerificationCode { get; set; }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new CitizenLinkRequestEntity(this, circularReferenceHelperDictionary);
		}

		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			await User.SafeEncrypt(encryptor);
			await Citizen.SafeEncrypt(encryptor);
			await base.EncryptSelf(encryptor);
		}
		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			await User.SafeDecrypt(decryptor);
			await Citizen.SafeDecrypt(decryptor);
			await base.DecryptSelf(decryptor);
		}
	}
}
