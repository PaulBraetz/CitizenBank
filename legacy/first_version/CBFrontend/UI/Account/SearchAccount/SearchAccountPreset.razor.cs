using CBData.Abstractions;

namespace CBFrontend.UI.Account.SearchAccount
{
    public partial class SearchAccountPreset
	{
		[Parameter]
		public EventCallback<IAccountEntity> ValueChanged { get; set; }
		[Parameter]
		public IAccountEntity Value { get; set; }

		private IAccountEntity value
		{
			get => Value;
			set => ValueChanged.InvokeAsync(Value = value);
		}
	}
}
