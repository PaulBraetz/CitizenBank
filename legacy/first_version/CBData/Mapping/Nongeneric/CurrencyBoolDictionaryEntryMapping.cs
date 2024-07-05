using PBData.Mapping;
using static CBData.Entities.CurrencyBoolDictionaryEntity;

namespace CBData.Mapping.Nongeneric
{
	class CurrencyBoolDictionaryEntryMapping : MappingBase<CurrencyBoolDictionaryEntryEntity>
	{
		public CurrencyBoolDictionaryEntryMapping()
		{
			References(m => m.Key);
			Map(m => m.Value);
		}
	}
}
