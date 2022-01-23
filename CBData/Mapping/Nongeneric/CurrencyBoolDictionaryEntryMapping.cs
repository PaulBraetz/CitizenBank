using PBData.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CBData.Entities.CurrencyBoolDictionaryEntity;

namespace CBData.Mapping.Nongeneric
{
	class CurrencyBoolDictionaryEntryMapping:MappingBase<CurrencyBoolDictionaryEntryEntity>
	{
		public CurrencyBoolDictionaryEntryMapping()
		{
			References(m => m.Key);
			Map(m=>m.Value);
		}
	}
}
