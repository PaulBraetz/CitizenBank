namespace CBCommon.Enums
{
	public static class LogisticsEnums
	{
		public enum OrderStatus
		{
			Open,
			Underway,
			Error,
			Cancelled,
			Delete,
			Completed
		}
		public enum OrderType
		{
			Miscellaneous,
			Cargo,
			Vehicles,
			Personnel,
			Ships,
			Refueling
		}
	}
}
