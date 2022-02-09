using CBApplication.Services.Abstractions;
using CitizenBank.Client.Services;
using PBClient.Access;
using PBClient.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Injection;

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
