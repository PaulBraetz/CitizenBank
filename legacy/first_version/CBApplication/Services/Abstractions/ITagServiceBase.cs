using CBData.Entities;

namespace CBApplication.Services.Abstractions
{
    public interface ITagServiceBase : IService
	{
		TagEntity GetTag(String name);
	}
}
