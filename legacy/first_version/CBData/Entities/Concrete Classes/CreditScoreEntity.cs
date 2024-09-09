namespace CBData.Entities
{
    public class CreditScoreEntity : EntityBase
	{
		public CreditScoreEntity()
		{
		}
		protected CreditScoreEntity(CreditScoreEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			DiscrepancyProbability = from.DiscrepancyProbability;
			DiscrepancyValueAverage = from.DiscrepancyValueAverage;
		}

		public virtual Decimal DiscrepancyProbability { get; set; }
		public virtual Decimal DiscrepancyValueAverage { get; set; }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new CreditScoreEntity(this, circularReferenceHelperDictionary);
		}
	}
}
