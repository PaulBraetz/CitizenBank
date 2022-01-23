using CBData.Entities;

using PBData.Mapping;

namespace CBData.Mapping
{
	public sealed class CitizenMapping : NamedMappingBase<CitizenEntity>
	{
		public CitizenMapping()
		{
			References(m => m.Owner);
		}
	}
}
