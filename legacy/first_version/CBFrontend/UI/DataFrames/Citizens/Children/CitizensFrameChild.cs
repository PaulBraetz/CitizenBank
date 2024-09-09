namespace CBFrontend.UI.DataFrames.Citizens.Children
{
    public class CitizensFrameChild : SessionChild
	{
		[CascadingParameter]
		protected CitizensFrame CitizensParent { get; set; }
	}
}
