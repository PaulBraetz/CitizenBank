using Microsoft.AspNetCore.Components;

using PBFrontend.UI.Authorization;

namespace CBFrontend.UI.DataFrames.Requests.Children
{
	public class CitizenRequestsChild : SessionChild
	{
		[CascadingParameter]
		protected CitizenRequestsFrame CitizenRequestsParent { get; set; }
	}
}
