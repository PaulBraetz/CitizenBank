using CBApplication.Services.Abstractions;

using CBData.Entities;

using Microsoft.AspNetCore.Components;
using PBFrontend.UI.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CBFrontend.UI.Details
{
	public partial class CitizenDetails
	{
		[Parameter]
		public CitizenEntity Citizen { get; set; }

		private readonly Dictionary<String, Object> attributes = new Dictionary<String, Object>
		{
			{"class","text-success clickable" }
		};
	}
}
