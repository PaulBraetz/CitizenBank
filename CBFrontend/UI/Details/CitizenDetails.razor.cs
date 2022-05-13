
using CBData.Entities;

using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace CBFrontend.UI.Details
{
	public partial class CitizenDetails
	{
		[Parameter]
		public CitizenEntity Citizen { get; set; }

		private readonly Dictionary<String, Object> attributes = new()
		{
			{ "class", "text-success clickable" }
		};
	}
}
