using CBData.Abstractions;
using CBData.Entities;
using Microsoft.AspNetCore.Components;
using PBApplication.Extensions;
using PBApplication.Requests;
using PBApplication.Requests.Abstractions;
using PBApplication.Responses;
using PBApplication.Responses.Abstractions;
using PBApplication.Services.Abstractions;
using PBApplication.Utilities;
using PBData.Entities;
using PBFrontend.UI.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CBFrontend.UI.DataFrames.Session
{
	partial class BankingSession : SessionChild
	{
		[Parameter]
		public RenderFragment ChildContent { get; set; }

		private Guid lastCitizen { get; set; }
		public CitizenEntity CurrentCitizen { get; private set; }

		private HashSet<CitizenEntity> citizens;
		public IEnumerable<CitizenEntity> Citizens
		{
			get
			{
				if (citizens == null)
				{
					yield break;
				}
				foreach (var citizen in citizens)
				{
					yield return citizen;
				}
			}
		}

		private Guid lastAccount { get; set; }
		public IAccountEntity CurrentAccount { get; private set; }

		private HashSet<IAccountEntity> accounts;
		public IEnumerable<IAccountEntity> Accounts
		{
			get
			{
				if (accounts != null)
				{
					yield break;
				}
				foreach (var account in accounts)
				{
					yield return account;
				}
			}
		}

		public async Task SetCurrentCitizen(CitizenEntity citizen)
		{
			var valueId = citizen?.Id ?? Guid.Empty;
			if (valueId == Guid.Empty)
			{
				await UnsubscribeFromAll();
				CurrentCitizen = citizen;
				lastCitizen = valueId;
			}
			else if (lastCitizen != valueId)
			{
				Func<IAsUserGetPaginatedEncryptableRequest<IClaimService.GetHeldClaimsParameter>, Task<IGetPaginatedEncryptableResponse<IClaimService.ClaimDto<CitizenEntity, IAccountEntity>>>> pageFactory =
					SessionParent.ServiceContext.GetService<IClaimService>().GetHeldClaims<CitizenEntity, IAccountEntity>;

				var request = new AsUserGetPaginatedEncryptableRequest<IClaimService.GetHeldClaimsParameter>()
				{
					Page = 0,
					PerPage = PBCommon.Configuration.Settings.MaxPaginatedPerPage,
					Parameter = new IClaimService.GetHeldClaimsParameter()
					{
						HolderId = CurrentCitizen.Id
					}
				};

				var response = new GetPaginatedEncryptableResponse<IClaimService.ClaimDto<CitizenEntity, IAccountEntity>>();

				await ServiceUtilities.AccumulatePages<IAsUserGetPaginatedEncryptableRequest<IClaimService.GetHeldClaimsParameter>,
					IGetPaginatedEncryptableResponse<IClaimService.ClaimDto<CitizenEntity, IAccountEntity>>,
					IClaimService.ClaimDto<CitizenEntity, IAccountEntity>>(pageFactory, request, response);

				if (response.HasData())
				{
					await UnsubscribeFromAll();
					CurrentCitizen = citizen;
					lastCitizen = valueId;

					//TODO
					//await Subscribe(new EventSubscription(nameof(IEventfulClaimService.OnClaimDeleted), CurrentCitizen.HubId), removeCurrentCitizen);
					//await Subscribe(new EventSubscription(nameof))


					async Task removeCurrentCitizen()
					{
						await SetCurrentCitizen(null);
					}
					async Task updateCurrentCitizen(IClaimService.ClaimDto<UserEntity, CitizenEntity> claim)
					{
						if (!(claim.Rights.Contains(PBCommon.Configuration.Settings.OwnerRight) || claim.Rights.Contains(PBCommon.Configuration.Settings.AdminRight)))
						{
							await removeCurrentCitizen();
						}
					}
				}
			}
		}
	}
}
