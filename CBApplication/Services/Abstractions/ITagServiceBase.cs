using CBData.Entities;

using PBApplication.Services.Abstractions;
using System;

namespace CBApplication.Services.Abstractions
{
	public interface ITagServiceBase : IService
	{
		TagEntity GetTag(String name);
	}
}
