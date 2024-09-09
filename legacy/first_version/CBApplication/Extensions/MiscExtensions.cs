
using CBCommon.Extensions;

using CBData.Entities;

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
