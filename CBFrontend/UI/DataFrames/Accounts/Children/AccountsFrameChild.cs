
using CBFrontend.UI.DataFrames.Citizens.Children;

using Microsoft.AspNetCore.Components;

namespace CBFrontend.UI.DataFrames.Accounts.Children
{
	public class AccountsFrameChild : CitizensFrameChild
	{
		[CascadingParameter]
		protected AccountsFrame AccountsParent { get; set; }
	}
}
