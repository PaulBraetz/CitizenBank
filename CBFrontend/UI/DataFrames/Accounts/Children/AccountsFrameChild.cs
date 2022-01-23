using CBApplication.Services.Abstractions;

using CBFrontend.UI.DataFrames.Citizens.Children;

using Microsoft.AspNetCore.Components;

using PBApplication.Services.Abstractions;

using PBFrontend.UI.Authorization;

using System;

namespace CBFrontend.UI.DataFrames.Accounts.Children
{
	public class AccountsFrameChild : CitizensFrameChild
	{
		[CascadingParameter]
		protected AccountsFrame AccountsParent { get; set; }
	}
}
