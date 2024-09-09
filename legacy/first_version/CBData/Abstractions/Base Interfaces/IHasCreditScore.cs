using CBData.Entities;

namespace CBData.Abstractions
{
    public interface IHasCreditScore : IEntity
	{
		CreditScoreEntity CreditScore { get; }
	}
}
