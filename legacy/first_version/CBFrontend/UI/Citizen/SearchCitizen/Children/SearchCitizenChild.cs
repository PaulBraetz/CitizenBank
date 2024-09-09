namespace CBFrontend.UI.Citizen.SearchCitizen.Children
{
    public class SearchCitizenChild : SessionChild
	{
		[CascadingParameter]
		protected SearchCitizenFrame SearchCitizenParent { get; set; }
	}
}
