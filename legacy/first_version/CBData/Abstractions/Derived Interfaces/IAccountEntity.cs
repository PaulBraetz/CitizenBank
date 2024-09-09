using CBData.Entities;

namespace CBData.Abstractions
{
    public interface IAccountEntity : IHasTags, IHasPriorityTags, IHasName, IHasCreator<CitizenEntity>, IHasCreditScore, ISessionAttachment
	{
	}
}

