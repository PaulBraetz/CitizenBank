using CBData.Entities;

namespace CBData.Mapping
{
    internal class CurrencyMapping : PluralNamedMappingBase<CurrencyEntity>
	{
		public CurrencyMapping()
		{
			Map(m => m.IsActive);
			Map(m => m.IngameTax);
			References(m => m.Creator);
		}
	}
}
