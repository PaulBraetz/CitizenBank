namespace CBData.Entities
{
    public class CitizenMessageEntity : MessageEntityBase<CitizenEntity>
	{
		public CitizenMessageEntity(CitizenEntity creator, IEnumerable<CitizenEntity> recipients, LocalizableFormattableString message, TimeSpan lifeSpan, Boolean expiryPaused)
			: base(creator, recipients, message, lifeSpan, expiryPaused)
		{
		}

		public CitizenMessageEntity() { }
		protected CitizenMessageEntity(CitizenMessageEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
		}

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new CitizenMessageEntity(this, circularReferenceHelperDictionary);
		}
	}
}
