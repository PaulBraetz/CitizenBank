
using CBData.Entities;

using PBApplication.Events;
using PBApplication.Responses;
using PBApplication.Services.Abstractions;
using System.Collections.Generic;

namespace CBApplication.Services.Abstractions
{
	public interface IEventfulLogisticsService : ILogisticsService, IEventfulService
	{
		//Payload : new order
		//Recipients : all sessions
		event ServiceEventHandler<ServiceEventArgs<LogisticsOrderEntity>> OnLogisticsOrderCreated;
		//Payload : new order
		//Recipients : affected order
		event ServiceEventHandler<ServiceEventArgs<LogisticsOrderEntity>> OnLogisticsOrderEdited;
		//Recipients : affected order
		event ServiceEventHandler<ServiceEventArgs> OnLogisticsOrderDeleted;
	}
}
