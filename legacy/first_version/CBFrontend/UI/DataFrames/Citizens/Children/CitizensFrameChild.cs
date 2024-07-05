using Microsoft.AspNetCore.Components;
using PBFrontend.UI.Authorization;

namespace CBFrontend.UI.DataFrames.Citizens.Children
{
	public class CitizensFrameChild : SessionChild
	{
		[CascadingParameter]
		protected CitizensFrame CitizensParent { get; set; }
	}
}
