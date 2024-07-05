using CBData.Entities;

using PBData.Mapping;

namespace CBData.Mapping
{
	public class AccountMappingBase<TAccount> : NamedMappingBase<TAccount>
		where TAccount : AccountEntityBase
	{
		public AccountMappingBase()
		{
			HasManyToMany(m => m.Tags);
			HasManyToMany(m => m.PriorityTags);
			References(m => m.CreditScore);
			References(m => m.Creator);
		}
	}
}
