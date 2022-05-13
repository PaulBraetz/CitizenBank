using CBData.Entities;

using PBData.Abstractions;

namespace CBData.Abstractions
{
	public interface IAccountEntity : IHasTags, IHasPriorityTags, IHasName, IHasCreator<CitizenEntity>, IHasCreditScore, ISessionAttachment
	{
	}
}

