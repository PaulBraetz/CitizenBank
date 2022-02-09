

using CBApplication.Requests;
using CBApplication.Requests.Abstractions;

using CBData.Abstractions;
using CBData.Entities;

using PBApplication.Events;
using PBApplication.Responses;
using PBApplication.Responses.Abstractions;
using PBApplication.Services.Abstractions;
using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;
using PBData.Abstractions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PBCommon.Enums;

namespace CBApplication.Services.Abstractions
{
	public interface IDepartmentServiceBase : IService
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
			public virtual ICollection<Guid> MembersIds { get; set; }
			public virtual ICollection<Guid> AdminsIds { get; set; }
			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				CreatorId = await decryptor.Decrypt(CreatorId);
				ExcludeIds = await decryptor.Decrypt(ExcludeIds);
				TagsIds = await decryptor.Decrypt(TagsIds);
				PriorityTagsIds = await decryptor.Decrypt(PriorityTagsIds);
				MembersIds = await decryptor.Decrypt(MembersIds);
				AdminsIds = await decryptor.Decrypt(AdminsIds);
			}
			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				CreatorId = await encryptor.Encrypt(CreatorId);
				ExcludeIds = await encryptor.Encrypt(ExcludeIds);
				TagsIds = await encryptor.Encrypt(TagsIds);
				PriorityTagsIds = await encryptor.Encrypt(PriorityTagsIds);
				MembersIds = await encryptor.Encrypt(MembersIds);
				AdminsIds = await encryptor.Encrypt(AdminsIds);
			}
		}
		Task<IGetPaginatedEncryptableResponse<IDepartmentEntity>> SearchDepartments(IAsAccountGetPaginatedEncryptableRequest<SearchDepartmentsParameterBase> request);

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
	public interface IDepartmentService : IDepartmentServiceBase, IEventfulService
	{
		//Payload : new department
		//Recipients : superadmins
		event ServiceEventHandler<ServiceEventArgs<SubDepartmentEntity>> OnDepartmentCreated;
		//Recipients : affected department
		event ServiceEventHandler<ServiceEventArgs> OnDepartmentDeleted;
		//Payload : admin
		//Recipients : affected department
		event ServiceEventHandler<ServiceEventArgs<CitizenEntity>> OnAdminResignedForDepartment;
		//Payload : department
		//Recipients : affected admin
		event ServiceEventHandler<ServiceEventArgs<IDepartmentEntity>> OnAdminResignedForAdmin;
		//Payload : new admin
		//Recipients : affected department
		event ServiceEventHandler<ServiceEventArgs<CitizenEntity>> OnAdminRecruitedForDepartment;
		//Payload : department
		//Recipients : affected admin
		event ServiceEventHandler<ServiceEventArgs<IDepartmentEntity>> OnAdminRecruitedForAdmin;
		//Payload : new member
		//Recipients : affected department
		event ServiceEventHandler<ServiceEventArgs<AccountEntityBase>> OnMemberRecruitedForDepartment;
		//Payload : department
		//Recipients : affected member
		event ServiceEventHandler<ServiceEventArgs<IDepartmentEntity>> OnMemberRecruitedForMember;
		//Payload : member
		//Recipients : affected department
		event ServiceEventHandler<ServiceEventArgs<AccountEntityBase>> OnMemberResignedForDepartment;
		//Payload : department
		//Recipients : affected member
		event ServiceEventHandler<ServiceEventArgs<IDepartmentEntity>> OnMemberResignedForMember;
		sealed class OnOrgsUpdatedData : EncryptableBase<Guid>
		{
			public ICollection<OrgEntity> Orgs { get; set; }

			protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
			{
				await Orgs.SafeDecrypt(decryptor);
			}

			protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
			{
				await Orgs.SafeEncrypt(encryptor);
			}
		}
		//Payload : orgs where current citizen is admin
		//Recipients : requested citizen
		event ServiceEventHandler<ServiceEventArgs<OnOrgsUpdatedData>> OnOrgsAdminsUpdated;
		//Payload : orgs where current account is member
		//Recipients : requested account
		event ServiceEventHandler<ServiceEventArgs<OnOrgsUpdatedData>> OnOrgsMembersUpdated;
		//Payload : new tag
		//Recipients : affected department
		event ServiceEventHandler<ServiceEventArgs<TagEntity>> OnTagAdded;
		//Payload : tag
		//Recipients : affected department
		event ServiceEventHandler<ServiceEventArgs<TagEntity>> OnTagRemoved;
	}
}
