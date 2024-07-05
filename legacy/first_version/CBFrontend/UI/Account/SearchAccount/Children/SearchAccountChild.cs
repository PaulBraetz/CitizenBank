using Microsoft.AspNetCore.Components;

using PBFrontend.UI.Authorization;

namespace CBFrontend.UI.Account.SearchAccount.Children
{
	public class SearchAccountChild : SessionChild
	{
		[CascadingParameter]
		protected SearchAccountFrame SearchAccountParent { get; set; }
	}
}
