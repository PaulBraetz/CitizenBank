using CBData.Entities;

using PBData.Mapping;

namespace CBData.Mapping
{
	public sealed class LogisticsOrderMapping : ExpiringMappingBase<LogisticsOrderEntity>
	{
		public LogisticsOrderMapping()
		{
			Map(m => m.Status).CustomType<CBCommon.Enums.LogisticsEnums.OrderStatus>();
			Map(m => m.Deadline);
			Map(m => m.Origin).Length(32768);
			Map(m => m.Target).Length(32768);
			Map(m => m.Details).Length(32768);
			Map(m => m.Verification).Length(32768);
			References(m => m.Client);
			Map(m => m.Type).CustomType<CBCommon.Enums.LogisticsEnums.OrderType>();
		}
	}
}
