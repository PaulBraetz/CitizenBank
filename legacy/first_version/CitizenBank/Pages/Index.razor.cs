﻿using Microsoft.AspNetCore.Components;

using PBApplication.Services.Abstractions;

using PBFrontend.UI.Authorization;
using System;
using System.Threading.Tasks;

namespace CitizenBank.Pages
{
	public partial class Index : SessionChild
	{
		[Parameter]
		public String Verification { get; set; }

		protected override async Task OnParametersSetAndSessionInitializedAsync()
		{
			if (Verification != null)
			{
				await SessionParent.ServiceContext.GetService<ICUDService>().Verify(new ICUDService.VerifyRequest()
				{
					VerificationCode = Verification
				});
			}

		}
	}
}
