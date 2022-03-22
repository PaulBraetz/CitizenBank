using CBApplication.Requests;
using CBApplication.Services.Abstractions;

using CBData.Entities;
using CBFrontend.UI.DataFrames.Citizens.Children;
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
	public partial class LogisticsOrderDetails : CitizensFrameChild
	{
		[Parameter]
		public LogisticsOrderEntity Order { get; set; }

		private CssColor HeaderColor
		{
			get
			{
				return Order.Status switch
				{
					OrderStatus.Open => CssColor.Blau,
					OrderStatus.Delete => CssColor.Pink,
					OrderStatus.Underway => CssColor.Orange,
					OrderStatus.Error => CssColor.Grau,
					OrderStatus.Completed => CssColor.Gruen,
					OrderStatus.Cancelled => CssColor.Rot,
					_ => CssColor.Grau,
				};
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
			await SessionParent.ServiceContext.GetService<IEventfulLogisticsService>().EditLogisticsOrder(new AsCitizenEncryptableRequest<IEventfulLogisticsService.EditLogisticsOrderParameter>()
			{
				AsUserId = SessionParent.Session.User.Id,
				AsCitizenId = CitizensParent.CurrentCitizen.Id,
				Parameter = new()
				{
					LogisticsOrderId = Order.Id,
					Status = status
				}
			});
		}
	}
}
