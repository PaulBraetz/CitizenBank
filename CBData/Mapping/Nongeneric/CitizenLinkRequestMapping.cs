using CBData.Entities;

using PBData.Mapping;

namespace CBData.Mapping
{
	internal class CitizenLinkRequestMapping : ExpiringMappingBase<CitizenLinkRequestEntity>
	{
		public CitizenLinkRequestMapping()
		{
			Map(m => m.VerificationCode).Length(32768);
			Map(m => m.CitizenName).Length(32768);
			References(m => m.Owner);
		}
	}
}
