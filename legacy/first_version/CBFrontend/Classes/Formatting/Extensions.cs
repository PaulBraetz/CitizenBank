using static CBFrontend.Classes.Formatting.Enums;

namespace CBFrontend.Classes.Formatting
{
    internal static class Extensions
	{
		public static String ToCssString(this CssColor color)
		{
			return $"cb-{color.ToLowerInvariantString()}";
		}
	}
}
