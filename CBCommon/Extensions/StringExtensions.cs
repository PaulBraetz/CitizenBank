using PBCommon.Extensions;
using System;
using System.Text.RegularExpressions;

using static CBCommon.Settings.CitizenBank;

namespace CBCommon.Extensions
{
	public static class StringExtensions
	{
		public static Boolean IsValidHandle(this String strToCheck)
		{
			return !String.IsNullOrWhiteSpace(strToCheck) && new Regex(@"^[A-Za-z0-9_-]{" + CHARS_MIN_ACCOUNT + "," + CHARS_MAX_ACCOUNT + "}$").IsMatch(strToCheck);
		}

		public static Boolean IsValidCurrencyName(this String strToCheck)
		{
			return strToCheck.IsWithinLimitsAndAlphanumeric(CHARS_MIN_CURRENCY - 1, CHARS_MAX_CURRENCY + 1);
		}

		public static Boolean IsValidTagName(this String strToCheck)
		{
			return strToCheck.IsWithinLimitsAndAlphanumeric(CHARS_MIN_TAG - 1, CHARS_MAX_TAG + 1);
		}

		public static Boolean IsValidSubDepartmentName(this String strToCheck)
		{
			return strToCheck.IsWithinLimitsAndAlphanumeric(CHARS_MIN_SUBDEPARTMENT - 1, CHARS_MAX_SUBDEPARTMENT + 1);
		}

		public static Boolean IsValidOrgName(this String strToCheck)
		{
			return strToCheck.IsWithinLimits(CHARS_MIN_ORG - 1, CHARS_MAX_ORG + 1);
		}

		public static Boolean IsValidSID(this String strToCheck)
		{
			return strToCheck.IsWithinLimitsAndAlphanumeric(CHARS_MIN_SID - 1, CHARS_MAX_SID + 1);
		}

		public static Boolean IsValidUsage(this String strToCheck)
		{
			return String.IsNullOrEmpty(strToCheck) || new Regex(@"^[A-Za-z0-9_@:;.,+#*!?\\§$€%&/()=\n -]{" + CHARS_MIN_USAGE + "," + CHARS_MAX_USAGE + "}$").IsMatch(strToCheck);
		}

		public static String ToVerifyLink(this String code)
		{
			return $"{PBCommon.Configuration.Settings.Url}/v/{code}";
		}
	}
}
