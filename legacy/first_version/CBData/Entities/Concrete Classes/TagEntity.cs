using PBData.Entities;

using System;
using System.Collections.Generic;

namespace CBData.Entities
{
	public class TagEntity : NamedEntityBase
	{
		public TagEntity(String text) : base(text)
		{
		}

		public TagEntity() { }
		protected TagEntity(TagEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
		}

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new TagEntity(this, circularReferenceHelperDictionary);
		}
	}
}
