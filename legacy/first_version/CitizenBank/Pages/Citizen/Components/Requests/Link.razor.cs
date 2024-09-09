using CBApplication.Services.Abstractions;

using static CBApplication.Services.Abstractions.ICitizenService;

namespace CitizenBank.Pages.Citizen.Components.Requests
{
    public partial class Link : LoadingChild
	{
		[CascadingParameter]
		protected SessionFrame SessionParent { get; set; }

		private IResponse linkNewResponse = new Response();
		private String linkNewInputValue;

		private async Task LinkNew(String name)
		{
			linkNewResponse = await LoadingParent.Load(() => SessionParent.ServiceContext.GetService<IEventfulCitizenService>().CreateCitizenLinkRequest(new AsUserRequest<CreateCitizenLinkRequestParameter>()
			{
				AsUserId = SessionParent.Session.User.Id,
				Parameter = new()
				{
					Name = name
				}
			}));
			await InvokeAsync(StateHasChanged);
		}
		private async Task LinkNew()
		{
			await LinkNew(linkNewInputValue);
		}
	}
}
