namespace CBData.Abstractions
{
    public interface IDepartmentSettingsEntity : ISettingsEntity
	{
		public Boolean InviteOnly { get; set; }
	}
}
