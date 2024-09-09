using CBApplication.Services.Abstractions;

using CitizenBank.Client.Services;

namespace CitizenBank.Client.Context
{
    public sealed class CBClientServiceContext : ClientServiceContext
	{
		public CBClientServiceContext(WebClient webClient) : base(webClient)
		{
			RegisterTypeToServices<ICitizenService, ClientCitizenService>(new InjectionConstructor(this, webClient));
		}
	}
}
