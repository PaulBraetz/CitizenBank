using PBApplication.Extensions;
using PBData.Abstractions;
using PBData.Access;
using System.Collections.Generic;

namespace CBApplication.Extensions
{
	public static class ClaimExtensions
	{
		public static IEnumerable<THolder> GetMemberClaimsHolders<THolder>(this IEntity value, IConnection connection)
			where THolder : IEntity
		{
			return value.GetClaimsHolders<THolder>(connection, CBCommon.Settings.CitizenBank.MEMBER_RIGHT);
		}
		public static IEnumerable<IEntity> GetMemberClaimsHolders(this IEntity value, IConnection connection)
		{
			return value.GetMemberClaimsHolders<IEntity>(connection);
		}
	}
}
