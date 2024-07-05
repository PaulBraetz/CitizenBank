using System;
using System.Linq;

namespace CBCommon.Extensions
{
	public static class DecimalExtensions
	{
		public static String ToFormattedString(this Decimal d, Int32 decimalPlaces)
		{
			decimalPlaces = decimalPlaces > 27 ? 27 : decimalPlaces;
			Decimal newVal = Math.Truncate(d * (Decimal)Math.Pow(10, decimalPlaces)) / (Decimal)Math.Pow(10, decimalPlaces);
			String retVal = newVal.ToString();
			for (Int32 i = retVal.Length - (1 + decimalPlaces) - (d - Math.Truncate(d) > 0 ? 3 : 0); i > 0; i -= 3)
			{
				retVal = retVal.Insert(i, " ");
			}
			return (newVal == 0 && d > 0 ? "> " : newVal == 0 && d < 0 ? "< " : Math.Abs(d) > Math.Abs(newVal) ? "~ " : "") + retVal + (newVal == 0 ? ",00" : "");
		}
		public static String ToFormattedString(this Decimal d)
		{
			return d.ToFormattedString(2);
		}

		public static String ToFormattedString(this Decimal d, Int32 decimalPlaces, String unit)
		{
			return (d > 0 ? (unit.Equals("%") ? d * 100 : d).ToFormattedString(decimalPlaces) : "0") + " " + unit;
		}

		public static String ToFormattedString(this Decimal d, String unit)
		{
			return d.ToFormattedString(2, unit);
		}

		public static Decimal RoundCIG(this Decimal d)
		{
			return Math.Ceiling(d);
		}

		public static String Trim(this Decimal d)
		{
			String retVal = d.ToString().Trim('0');
			if (retVal.Contains(','))
			{
				if (retVal.First().Equals(','))
				{
					retVal = $"0{retVal}";
				}
				else if (retVal.Last().Equals(','))
				{
					retVal += "0";
				}
			}
			else if (retVal.Equals(String.Empty))
			{
				return "0,0";
			}
			return retVal;
		}
	}
}
