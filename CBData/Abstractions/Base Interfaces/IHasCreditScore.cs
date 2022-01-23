using CBData.Entities;

using PBData.Abstractions;

namespace CBData.Abstractions
{
	public interface IHasCreditScore : IEntity
	{
		CreditScoreEntity CreditScore { get; }
	}
}
