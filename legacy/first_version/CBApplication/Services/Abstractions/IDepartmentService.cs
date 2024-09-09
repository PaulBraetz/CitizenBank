using CBApplication.Requests.Abstractions;

using CBData.Abstractions;
using CBData.Entities;

namespace CBApplication.Services.Abstractions
{
    public interface IDepartmentService : IService
	{
		sealed class CreateDepartmentParameter : EncryptableBase<Guid>
		{
			public String Name { get; set; }
			public Guid SuperDepartmentId { get; set; }
			public AccessibilityType Accessibility { get; set; }

			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				SuperDepartmentId = await decryptor.Decrypt(SuperDepartmentId);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				SuperDepartmentId = await encryptor.Encrypt(SuperDepartmentId);
			}
		}
		Task<IResponse> CreateDepartment(IAsCitizenEncryptableRequest<CreateDepartmentParameter> request);

		sealed class DeleteDepartmentParameter : EncryptableBase<Guid>
		{
			public Guid DepartmentId { get; set; }
			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				DepartmentId = await decryptor.Decrypt(DepartmentId);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				DepartmentId = await encryptor.Encrypt(DepartmentId);
			}
		}
		Task<IResponse> DeleteDepartment(IAsCitizenEncryptableRequest<DeleteDepartmentParameter> request);

		sealed class EditDepartmentAdminshipParameter : EncryptableBase<Guid>
		{
			public Guid DepartmentId { get; set; }
			public Guid AdminId { get; set; }
			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				DepartmentId = await decryptor.Decrypt(DepartmentId);
				AdminId = await decryptor.Decrypt(AdminId);
			}
			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				DepartmentId = await encryptor.Encrypt(DepartmentId);
				AdminId = await encryptor.Encrypt(AdminId);
			}
		}
		Task<IResponse> ResignAdminFromDepartment(IAsCitizenEncryptableRequest<EditDepartmentAdminshipParameter> request);
		Task<IResponse> RecruitAdminIntoDepartment(IAsCitizenEncryptableRequest<EditDepartmentAdminshipParameter> request);

		sealed class EditDepartmentMembershipParameter : EncryptableBase<Guid>
		{
			public Guid DepartmentId { get; set; }
			public Guid MemberId { get; set; }
			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				DepartmentId = await decryptor.Decrypt(DepartmentId);
				MemberId = await decryptor.Decrypt(MemberId);
			}
			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				DepartmentId = await encryptor.Encrypt(DepartmentId);
				MemberId = await encryptor.Encrypt(MemberId);
			}
		}
		Task<IResponse> RecruitMemberIntoDepartment(IAsCitizenEncryptableRequest<EditDepartmentMembershipParameter> request);
		Task<IResponse> ResignMemberFromDepartment(IAsCitizenEncryptableRequest<EditDepartmentMembershipParameter> request);

		// SubDepartmentEntity GetSubDepartment(IAsCitizenRequest<GetSubDepartmentParameter> request);
		// OrgEntity GetOrg(IAsCitizenRequest<GetOrgParameter> request);

		Task<IGetPaginatedEncryptableResponse<IDepartmentEntity>> GetDepartmentsForMember(IAsAccountRequest request);

		Task<IGetPaginatedEncryptableResponse<IDepartmentEntity>> GetDepartmentsForAdmin(IAsCitizenRequest request);

		Task<IResponse> UpdateOrgs(IAsAccountRequest request);

		sealed class EditDepartmentTagsParameter : EncryptableBase<Guid>
		{
			public Guid DepartmentId { get; set; }
			public String TagName { get; set; }
			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				DepartmentId = await decryptor.Decrypt(DepartmentId);
			}
			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				DepartmentId = await encryptor.Encrypt(DepartmentId);
			}
		}
		Task<IResponse> AddTagToDepartment(IAsCitizenEncryptableRequest<EditDepartmentTagsParameter> request);
		Task<IResponse> RemoveTagFromDepartment(IAsCitizenEncryptableRequest<EditDepartmentTagsParameter> request);

		abstract class SearchDepartmentsParameterBase : EncryptableBase<Guid>
		{
			public AccessibilityType? Accessibility { get; set; }
			public Boolean? InviteOnly { get; set; }
			public String Name { get; set; }
			public Guid? CreatorId { get; set; }
			public ICollection<String> ExcludeNames { get; set; }
			public IEnumerable<Guid> ExcludeIds { get; set; }
			public ICollection<Guid> TagsIds { get; set; }
			public ICollection<Guid> PriorityTagsIds { get; set; }
			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				CreatorId = await decryptor.Decrypt(CreatorId);
				ExcludeIds = await decryptor.Decrypt(ExcludeIds);
				TagsIds = await decryptor.Decrypt(TagsIds);
				PriorityTagsIds = await decryptor.Decrypt(PriorityTagsIds);
			}
			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				CreatorId = await encryptor.Encrypt(CreatorId);
				ExcludeIds = await encryptor.Encrypt(ExcludeIds);
				TagsIds = await encryptor.Encrypt(TagsIds);
				PriorityTagsIds = await encryptor.Encrypt(PriorityTagsIds);
			}
		}
		sealed class SearchOrgsParameter : SearchDepartmentsParameterBase
		{
			public String SID { get; set; }
		}
		Task<IGetPaginatedEncryptableResponse<OrgEntity>> SearchOrgs(IAsAccountGetPaginatedEncryptableRequest<SearchOrgsParameter> request);

		sealed class SearchSubDepartmentsParameter : SearchDepartmentsParameterBase
		{
		}
		Task<IGetPaginatedEncryptableResponse<SubDepartmentEntity>> SearchSubDepartments(IAsAccountGetPaginatedEncryptableRequest<SearchSubDepartmentsParameter> request);
	}
}
