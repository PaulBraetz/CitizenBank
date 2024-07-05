using PBFrontend.Classes.Formatting;
using PBFrontend.Classes.Formatting.Abstractions;

namespace CitizenBank.Pages.Citizen.Components.Requests
{
	partial class Requests
	{
		private static readonly ICss footerCss = new CssCollection<ICss>()
			.SetNew<DisplayCss>(css => css.V(PBFrontend.Classes.Enums.DisplayValue.InlineFlex))
			.ToImmutable();
	}
}
