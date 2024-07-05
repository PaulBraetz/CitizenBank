using CBData.Entities;

using PBData.Mapping;

namespace CBData.Mapping
{
	internal class DepositAccountReferenceMapping : MappingBase<DepositAccountReferenceEntity>
	{
		public DepositAccountReferenceMapping()
		{
			Map(m => m.AbsoluteBalance);
			Map(m => m.RelativeBalance);
			Map(m => m.AbsoluteLimit);
			Map(m => m.RelativeLimit);
			Map(m => m.CalculatedAbsoluteLimit);
			Map(m => m.CalculatedRelativeLimit);
			Map(m => m.Saturation);
			Map(m => m.IsActive);
			Map(m => m.UseAsForwarding);
			References(m => m.Currency);
			References(m => m.ReferencedAccount);
		}
	}
}
