using CBData.Entities;

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
