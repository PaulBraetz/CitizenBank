using Microsoft.AspNetCore.Components;

using PBFrontend.UI.Authorization;

namespace CBFrontend.UI.Citizen.SearchCitizen.Children
{
	public class SearchCitizenChild : SessionChild
	{
		[CascadingParameter]
		protected SearchCitizenFrame SearchCitizenParent { get; set; }
	}
}
