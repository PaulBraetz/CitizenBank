
using CBData.Entities;

namespace CBFrontend.UI.Citizen.SearchCitizen
{
    public partial class SearchCitizenPreset
	{
		[Parameter]
		public EventCallback<CitizenEntity> ValueChanged { get; set; }
		[Parameter]
		public CitizenEntity Value { get; set; }

		private CitizenEntity value
		{
			get => Value;
			set => ValueChanged.InvokeAsync(Value = value);
		}
	}
}
