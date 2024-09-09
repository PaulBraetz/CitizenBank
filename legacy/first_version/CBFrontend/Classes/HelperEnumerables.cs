using static CBCommon.Enums.LogisticsEnums;

namespace CBFrontend.Classes
{
    public static class HelperEnumerables
	{
		public static readonly IEnumerable<SelectInput<OrderStatus>.OptionModel> OrderStatusOptions = Enum.GetValues<OrderStatus>()
			.Select(s => new SelectInput<OrderStatus>.OptionModel(s, s.ToString()))
			.ToList()
			.AsReadOnly();
		public static readonly IEnumerable<SelectInput<OrderType>.OptionModel> OrderTypeOptions = Enum.GetValues<OrderType>()
			.Select(s => new SelectInput<OrderType>.OptionModel(s, s.ToString()))
			.ToList()
			.AsReadOnly();
	}
}
