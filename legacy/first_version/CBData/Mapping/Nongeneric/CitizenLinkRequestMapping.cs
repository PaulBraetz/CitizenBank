using CBData.Entities;

using PBData.Mapping;

namespace CBData.Mapping
{
	internal class CitizenLinkRequestMapping : ExpiringMappingBase<CitizenLinkRequestEntity>
	{
		public CitizenLinkRequestMapping()
		{
			Map(m => m.VerificationCode).Length(32768);
			References(m => m.Citizen);
			References(m => m.User);
		}
	}
}
