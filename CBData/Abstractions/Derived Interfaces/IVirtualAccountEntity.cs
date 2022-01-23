
using CBData.Entities;

using PBData.Abstractions;

using System.Collections.Generic;

namespace CBData.Abstractions
{
	public interface IVirtualAccountEntity : IAccountEntity, IHasCreator<CitizenEntity>
	{
		ICollection<DepositAccountReferenceEntity> DepositReferences { get; set; }
	}
}
