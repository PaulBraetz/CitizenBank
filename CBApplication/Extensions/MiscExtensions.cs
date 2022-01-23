using CBApplication.Services.Abstractions;

using CBCommon.Extensions;

using CBData.Entities;

using PBApplication.Responses.Abstractions;
using PBApplication.Services.Abstractions;

using PBData.Abstractions;

using System;
using System.Collections.Generic;
using System.Linq;

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
