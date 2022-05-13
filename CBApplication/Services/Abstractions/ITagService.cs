using CBApplication.Requests.Abstractions;
using CBData.Entities;
using PBApplication.Responses.Abstractions;
using PBApplication.Services.Abstractions;
using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CBApplication.Services.Abstractions
{
	public interface ITagService : IService
	{
		sealed class SearchTagsParameter : EncryptableBase<Guid>
		{
			public String Name { get; set; }
			public IEnumerable<Guid> ExcludeIds { get; set; }
			public IEnumerable<String> ExcludeTexts { get; set; }
			public Guid? PriorityTagsProviderId { get; set; }

			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				PriorityTagsProviderId = await decryptor.Decrypt(PriorityTagsProviderId);
				ExcludeIds = await decryptor.Decrypt(ExcludeIds);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				PriorityTagsProviderId = await encryptor.Encrypt(PriorityTagsProviderId);
				ExcludeIds = await encryptor.Encrypt(ExcludeIds);
			}
		}
		sealed class SearchTagModel : EncryptableBase<Guid>
		{
			public SearchTagModel(TagEntity tag, Boolean isPrioritized)
			{
				Tag = tag;
				IsPrioritized = isPrioritized;
			}

			public TagEntity Tag { get; set; }
			public Boolean IsPrioritized { get; set; }
			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				await Tag.SafeDecrypt(decryptor);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				await Tag.SafeEncrypt(encryptor);
			}
		}
		Task<IGetPaginatedEncryptableResponse<SearchTagModel>> SearchTags(IAsAccountGetPaginatedEncryptableRequest<SearchTagsParameter> request);

		Task<TagEntity> GetTag(String name);
	}
}
