using PBData.Entities;
using PBData.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static CBData.Entities.CurrencyBoolDictionaryEntity;

namespace CBData.Entities
{
	public class CurrencyBoolDictionaryEntity : EntityBase, IEnumerable<CurrencyBoolDictionaryEntryEntity>
	{
		public class CurrencyBoolDictionaryEntryEntity : EntityBase
		{
			public CurrencyBoolDictionaryEntryEntity()
			{
			}

			public CurrencyBoolDictionaryEntryEntity(CurrencyBoolDictionaryEntryEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
			{
				Key = from.Key.CloneAsT(circularReferenceHelperDictionary);
				Value = from.Value;
			}

			public CurrencyBoolDictionaryEntryEntity(CurrencyEntity key, Boolean value)
			{
				Key = key;
				Value = value;
			}

			public virtual CurrencyEntity Key { get; set; }
			public virtual Boolean Value { get; set; }

			public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
			{
				return new CurrencyBoolDictionaryEntryEntity(this, circularReferenceHelperDictionary);
			}
		}
		public CurrencyBoolDictionaryEntity(ICollection<CurrencyEntity> currencies, Boolean defaultValue = false)
		{
			DefaultValue = defaultValue;
			Values = currencies.Select(c => new CurrencyBoolDictionaryEntryEntity(c, DefaultValue)).ToArray();
		}

		public CurrencyBoolDictionaryEntity() { }
		protected CurrencyBoolDictionaryEntity(CurrencyBoolDictionaryEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			Values = from.Values.CloneAsT(circularReferenceHelperDictionary).ToArray();
			DefaultValue = from.DefaultValue;
		}

		public virtual Boolean DefaultValue { get; set; }
		public virtual ICollection<CurrencyBoolDictionaryEntryEntity> Values { get; set; }

		public virtual Boolean ContainsKey(CurrencyEntity key)
		{
			return Values.Any(v => v.Key.Equals(key));
		}
		public virtual CurrencyBoolDictionaryEntryEntity GetEntry(CurrencyEntity key)
		{
			return Values.Single(v => v.Key.Equals(key));
		}
		public virtual Boolean TrySetValue(CurrencyEntity key, Boolean newValue)
		{
			if (ContainsKey(key))
			{
				GetEntry(key).Value = newValue;
				return true;
			}
			return false;
		}
		public virtual Boolean TryRemoveValue(CurrencyEntity key)
		{
			return Values.Remove(GetEntry(key));
		}
		public virtual Boolean TryAddValue(CurrencyEntity key)
		{
			return TryAddValue(new CurrencyBoolDictionaryEntryEntity(key, DefaultValue));
		}
		public virtual Boolean TryAddValue(CurrencyEntity key, Boolean value)
		{
			return TryAddValue(new CurrencyBoolDictionaryEntryEntity(key, value));
		}
		public virtual Boolean TryAddValue(CurrencyBoolDictionaryEntryEntity value)
		{
			if (!ContainsKey(value.Key))
			{
				Values.Add(value);
				return true;
			}
			return false;
		}
		public virtual Boolean this[CurrencyEntity key]
		{
			get
			{
				if (ContainsKey(key))
				{
					return GetEntry(key).Value;
				}
				TryAddValue(key, DefaultValue);
				return this[key];
			}
			set
			{
				TryAddValue(key, value);
			}
		}

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new CurrencyBoolDictionaryEntity(this, circularReferenceHelperDictionary);
		}

		public virtual IEnumerator<CurrencyBoolDictionaryEntryEntity> GetEnumerator()
		{
			return Values.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return Values.GetEnumerator();
		}
	}
}
