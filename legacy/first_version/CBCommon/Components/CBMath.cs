using System;
using System.Collections.Generic;

namespace CBCommon.Components
{
	public static class CBMath
	{
		public static IEnumerable<Int32> PadOut(Int32 current, Int32 lowerLimit, Int32 upperLimit, Int32 padding)
		{
			Int32 displayedPagesCount = padding * 2 + 1;
			if (upperLimit <= displayedPagesCount)
			{
				for (Int32 i = lowerLimit; i <= Math.Min(upperLimit, lowerLimit + displayedPagesCount); i++)
				{
					yield return i;
				}
			}
			else
			{
				Int32 upperThreshold = current + padding;
				Int32 lowerThreshold = current - padding;
				if (upperThreshold > upperLimit)
				{
					upperThreshold = upperLimit;
					lowerThreshold = upperLimit - displayedPagesCount + 1;
				}
				if (lowerThreshold < lowerLimit)
				{
					lowerThreshold = lowerLimit;
					upperThreshold = lowerLimit + displayedPagesCount - 1;
				}
				for (Int32 i = lowerThreshold; i <= upperThreshold; i++)
				{
					yield return i;
				}
			}
		}
	}
}
