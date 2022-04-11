
using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;
using PBData.Entities;
using PBData.Extensions;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CBCommon.Enums.LogisticsEnums;

namespace CBData.Entities
{
	public class LogisticsOrderEntity : ExpiringEntityBase, PBData.Abstractions.IHasVerification
	{
		public LogisticsOrderEntity() { }
		public LogisticsOrderEntity(DateTimeOffset deadline, String target, CitizenEntity client, OrderType type, String details, String id)
			: base(CBCommon.Settings.Logistics.ORDER_LIFESPAN, true, true)
		{
			Deadline = deadline;
			Target = target;
			Client = client;
			Type = type;
			Details = details;
			Verification = id;
		}

		public LogisticsOrderEntity(LogisticsOrderEntity from, IDictionary<Guid, Object> circularReferenceHelperDictionary) : base(from, circularReferenceHelperDictionary)
		{
			Status = from.Status;
			Deadline = from.Deadline;
			Origin = from.Origin;
			Target = from.Target;
			Client = from.Client.CloneAsT(circularReferenceHelperDictionary);
			Type = from.Type;
			Details = from.Details;
			Verification = from.Verification;
		}

		public virtual OrderStatus Status { get; set; }
		public virtual DateTimeOffset Deadline { get; set; }
		public virtual String Origin { get; set; }
		public virtual String Target { get; set; }
		public virtual CitizenEntity Client { get; set; }
		public virtual OrderType Type { get; set; }
		public virtual String Details { get; set; }
		public virtual String Verification { get; set; }
		public virtual String OrderId { get => Verification; set => _ = value; }

		public override Object Clone(IDictionary<Guid, Object> circularReferenceHelperDictionary)
		{
			return new LogisticsOrderEntity(this, circularReferenceHelperDictionary);
		}

		protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
		{
			await Client.SafeEncrypt(encryptor);
			await base.EncryptSelf(encryptor);
		}

		protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
		{
			await Client.SafeDecrypt(decryptor);
			await base.DecryptSelf(decryptor);
		}
	}
}
