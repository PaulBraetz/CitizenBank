using CBData.Entities;

namespace CBData.Mapping
{
    internal class CreditScoreMapping : MappingBase<CreditScoreEntity>
	{
		public CreditScoreMapping()
		{
			Map(m => m.DiscrepancyProbability);
			Map(m => m.DiscrepancyValueAverage);
		}
	}
}
