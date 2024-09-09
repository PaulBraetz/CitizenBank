
using CBFrontend.UI.DataFrames.Citizens.Children;

namespace CBFrontend.UI.DataFrames.Accounts.Children
{
    public class AccountsFrameChild : CitizensFrameChild
	{
		[CascadingParameter]
		protected AccountsFrame AccountsParent { get; set; }
	}
}
