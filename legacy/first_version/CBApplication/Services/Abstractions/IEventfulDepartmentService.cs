
using CBData.Abstractions;
using CBData.Entities;

namespace CBApplication.Services.Abstractions
{
    public interface IEventfulDepartmentService : IDepartmentService, IEventfulService
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
		event ServiceEventHandler<ServiceEventArgs<IAccountEntity>> OnMemberRecruitedForDepartment;
		//Payload : department
		//Recipients : affected member
		event ServiceEventHandler<ServiceEventArgs<IDepartmentEntity>> OnMemberRecruitedForMember;
		//Payload : member
		//Recipients : affected department
		event ServiceEventHandler<ServiceEventArgs<IAccountEntity>> OnMemberResignedForDepartment;
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
