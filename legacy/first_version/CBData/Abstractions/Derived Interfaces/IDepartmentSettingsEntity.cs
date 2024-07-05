
using PBData.Abstractions;
using System;

namespace CBData.Abstractions
{
	public interface IDepartmentSettingsEntity : ISettingsEntity
	{
		public Boolean InviteOnly { get; set; }
	}
}
