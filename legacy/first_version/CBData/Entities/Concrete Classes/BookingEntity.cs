namespace CBData.Entities
{
    public class BookingEntity : EntityBase
	{
		public BookingEntity(Decimal value)
		{
			Value = value;
		}

		public BookingEntity() { }
		protected BookingEntity(BookingEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			Value = from.Value;
		}

		public virtual Decimal Value { get; }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new BookingEntity(this, circularReferenceHelperDictionary);
		}
	}
}
