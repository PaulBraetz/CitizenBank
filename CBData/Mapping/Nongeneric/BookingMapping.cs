using CBData.Entities;

using PBData.Mapping;

namespace CBData.Mapping
{
	internal class BookingMapping : MappingBase<BookingEntity>
	{
		public BookingMapping()
		{
			Map(m => m.Value);
		}
	}
}
