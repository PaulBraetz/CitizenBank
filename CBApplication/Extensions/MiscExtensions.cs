
using CBCommon.Extensions;

using CBData.Entities;

using System;

namespace CBApplication.Extensions
{
	public static class MiscExtensions
	{
		public static String ToFormattedCurrency(this Decimal val, CurrencyEntity currency)
		{
			return val.ToFormattedString(val == 1 ? currency.Name : currency.PluralName);
		}
	}
}
