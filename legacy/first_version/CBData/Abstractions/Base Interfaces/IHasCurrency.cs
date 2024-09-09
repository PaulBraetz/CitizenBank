using CBData.Entities;

namespace CBData.Abstractions
{
    public interface IHasCurrency<TCurrency> : IEntity
		where TCurrency : CurrencyEntity
	{
		TCurrency Currency { get; }
	}
}
