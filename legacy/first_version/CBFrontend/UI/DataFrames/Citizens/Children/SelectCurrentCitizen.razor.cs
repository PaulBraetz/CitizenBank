
using CBData.Entities;

using PBFrontend.UI.Input;
using System.Collections.Generic;
using System.Linq;

namespace CBFrontend.UI.DataFrames.Citizens.Children
{
	partial class SelectCurrentCitizen : CitizensFrameChild
	{
		private IEnumerable<SelectInput<CitizenEntity>.OptionModel> Options => CitizensParent.Citizens.Select(c => new SelectInput<CitizenEntity>.OptionModel(c, c.Name, false));
	}
}
