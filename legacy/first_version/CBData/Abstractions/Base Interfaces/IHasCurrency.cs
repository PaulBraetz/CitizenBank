using CBData.Entities;

using PBData.Abstractions;

namespace CBData.Abstractions
{
	public interface IHasCurrency<TCurrency> : IEntity
		where TCurrency : CurrencyEntity
	{
		TCurrency Currency { get; }
	}
}
