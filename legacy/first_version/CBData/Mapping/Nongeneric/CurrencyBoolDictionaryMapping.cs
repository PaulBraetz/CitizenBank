using CBData.Entities;

using PBData.Mapping;

namespace CBData.Mapping
{
	internal class CurrencyBoolDictionaryMapping : MappingBase<CurrencyBoolDictionaryEntity>
	{
		public CurrencyBoolDictionaryMapping()
		{
			Map(m => m.DefaultValue);
			HasMany(m => m.Values).Cascade.AllDeleteOrphan();
		}
	}
}
