using CBApplication.Services.Abstractions;

using CBData.Entities;

using Microsoft.AspNetCore.Components;

using PBApplication.Requests;
using PBFrontend.Extensions;
using PBFrontend.UI.Authorization;
using PBFrontend.UI.Input;
using PBShared.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static CBCommon.Enums.LogisticsEnums;
using static CBFrontend.Classes.Formatting.Enums;

namespace CBFrontend.UI.Logistics
{
	public partial class LogisticsOrderDetails : SessionChild
	{
		[Parameter]
		public LogisticsOrderEntity Order { get; set; }

		private CssColor HeaderColor
		{
			get
			{
				switch (Order.Status)
				{
					case OrderStatus.Open:
						return CssColor.Blau;
					case OrderStatus.Delete:
						return CssColor.Pink;
					case OrderStatus.Underway:
						return CssColor.Orange;
					case OrderStatus.Error:
						return CssColor.Grau;
					case OrderStatus.Completed:
						return CssColor.Gruen;
					case OrderStatus.Cancelled:
						return CssColor.Rot;
				}
				return CssColor.Grau;
			}
		}

		protected override async Task OnParametersSetAndSessionInitializedAsync()
		{
			await SubscribeOnce<LogisticsOrderEntity>(new EventSubscription(nameof(IEventfulLogisticsService.OnLogisticsOrderEdited), Order.HubId), onEdit);

			await base.OnInitializedAsync();
			void onEdit(LogisticsOrderEntity oder)
			{
				Order = oder;
				InvokeAsync(StateHasChanged);
			}
		}

		private async Task Edit(OrderStatus status)
		{
			await SessionParent.ServiceContext.GetService<IEventfulLogisticsService>().EditLogisticsOrder(new AsUserEncryptableRequest<IEventfulLogisticsService.EditLogisticsOrderParameter>()
			{
				AsUserId = SessionParent.Session.Owner.Id,
				Parameter = new()
				{
					LogisticsOrderId = Order.Id,
					Status = status
				}
			});
		}
	}
}
