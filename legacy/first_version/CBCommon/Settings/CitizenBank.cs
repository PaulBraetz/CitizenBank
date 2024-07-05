using System;

namespace CBCommon.Settings
{
	public static class CitizenBank
	{
		public static readonly String DefaultGeneratedMessage = "[Generated]";
		public static readonly Decimal MAX_CURRENCY_VAL = 1000000000000;
		public static readonly String CITIZEN_RIGHT = "Citizen";
		public static readonly String MEMBER_RIGHT = "Member";
		public static readonly Int32 CHARS_MIN_ACCOUNT = 4;
		public static readonly Int32 CHARS_MAX_ACCOUNT = 60;
		public static readonly Int32 CHARS_MIN_SUBDEPARTMENT = 5;
		public static readonly Int32 CHARS_MAX_SUBDEPARTMENT = 128;
		public static readonly Int32 CHARS_MIN_CURRENCY = 2;
		public static readonly Int32 CHARS_MAX_CURRENCY = 64;
		public static readonly Int32 CHARS_MIN_SID = 5;
		public static readonly Int32 CHARS_MAX_SID = 10;
		public static readonly Int32 CHARS_MIN_TAG = 3;
		public static readonly Int32 CHARS_MAX_TAG = 64;
		public static readonly Int32 CHARS_MIN_USAGE = 0;
		public static readonly Int32 CHARS_MAX_USAGE = 4096;
		public static readonly Int32 CHARS_MIN_ORG = 0;
		public static readonly Int32 CHARS_MAX_ORG = 50;
		public static String EMAIL_SERVER;
		public static String EMAIL_NOREPLY_USER;
		public static String EMAIL_NOREPLY_PASSWORD;
		public static String EMAIL_NOREPLY_ADDRESS;
		public static Int32 EMAIL_SMTP_PORT;
		public static Int32 EMAIL_POP3_PORT;
	}
}
