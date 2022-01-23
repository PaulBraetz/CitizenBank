using System;

namespace CBData.Abstractions
{
	public interface IVirtualAccountSettingsEntity : IAccountSettingsEntity
	{
		TimeSpan DepositForwardLifeSpan { get; set; }
		Decimal DefaultDepositAccountMapRelativeLimit { get; set; }
		Decimal DefaultDepositAccountMapAbsoluteLimit { get; set; }
	}
}
