using CBApplication.Requests;
using CBApplication.Requests.Abstractions;
using CBApplication.Services.Abstractions;

using CBCommon.Extensions;

using CBData.Abstractions;
using CBData.Entities;

using PBApplication.Events;
using PBApplication.Extensions;
using PBApplication.Responses;
using PBApplication.Responses.Abstractions;
using PBCommon.Validation;
using PBCommon.Validation.Abstractions;

using PBCommon.Extensions;

using PBData.Abstractions;
using PBData.Entities;
using PBData.Extensions;

using ScrapeX;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using static CBApplication.Services.Abstractions.IDepartmentService;
using static PBCommon.Enums;
using System.Threading.Tasks;
using static CBApplication.Services.Abstractions.IDepartmentServiceBase;
using PBApplication.Context.Abstractions;
using PBApplication.Services.Abstractions;

namespace CBApplication.Services
{
	public class DepartmentService : CBService, IDepartmentService
	{
		public DepartmentService(IServiceContext serviceContext) : base(serviceContext)
		{
			Observe<IDepartmentService>(this);
		}

		public event ServiceEventHandler<ServiceEventArgs<SubDepartmentEntity>> OnDepartmentCreated;
		public async Task<IResponse> CreateDepartment(IAsCitizenEncryptableRequest<CreateDepartmentParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				Lazy<IDepartmentEntity> superDepartment = Connection.GetSingle<IDepartmentEntity>(request.Parameter.SuperDepartmentId);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);

				void successAction()
				{
					var newDepartment = new SubDepartmentEntity(citizen.Value, request.Parameter.Name);
					var newSettings = new SubDepartmentSettingsEntity()
					{
						Accessibility = request.Parameter.Accessibility
					};
					Connection.Insert(newDepartment, newSettings);

					_ = GetService<IEventfulClaimService>().EnsureClaim(newDepartment, PBCommon.Configuration.Settings.OWNER_RIGHT, newSettings);

					superDepartment.Value.SubDepartments.Add(newDepartment);
					Connection.Update(superDepartment.Value);
					Connection.SaveChanges();

					var admins = superDepartment.Value.GetAdminClaimsHolders<IEntity>(Connection);

					OnDepartmentCreated.Invoke(Session, admins, newDepartment.CloneAsT());

					UserEntity user = GetUserEntity(request);
					LogIfAccessingAsDelegate(user, "created department: {0}", newDepartment.Name);
				}

				await FirstValidateAsCitizen(request, response)
					.NextEntityHoldsManager(citizen, superDepartment, Connection, ValidationField.Create(nameof(request.Parameter.SuperDepartmentId)))
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		private IEnumerable<SubDepartmentEntity> GetAllSubDepartments(SubDepartmentEntity department)
		{
			return new[] { department }.Concat(department.SubDepartments.SelectMany(s => GetAllSubDepartments(s)));
		}

		public event ServiceEventHandler<ServiceEventArgs> OnDepartmentDeleted;
		public async Task<IResponse> DeleteDepartment(IAsCitizenEncryptableRequest<DeleteDepartmentParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<SubDepartmentEntity> department = Connection.GetSingle<SubDepartmentEntity>(request.Parameter.DepartmentId);
				Lazy<IDepartmentEntity> superDepartment = Connection.GetFirst<IDepartmentEntity>(d => d.SubDepartments.Any(s => s.Id == department.Value.Id));

				void successAction()
				{
					IOrderedEnumerable<SubDepartmentEntity> subDepartments = GetAllSubDepartments(department.Value).OrderBy(d => d.CreationDate);
					foreach (SubDepartmentEntity subDepartment in subDepartments)
					{
						Connection.Delete(subDepartment);
						var messageService = GetService<IEventfulCBMessageService>();

						var members = subDepartment.GetClaimsHolders<AccountEntityBase>(Connection, CBCommon.Settings.CitizenBank.MEMBER_RIGHT);
						var admins = subDepartment.GetAdminClaimsHolders<CitizenEntity>(Connection);

						messageService.CreateAccountSelfMessages(members, new("{0} has deleted the subDepartmentartment {1}", citizen.Value.Name, subDepartment.Name));
						messageService.CreateCitizenMessages(citizen.Value, admins, new("I have deleted the subDepartmentartment {0}", subDepartment.Name));

						OnDepartmentDeleted.Invoke(subDepartment);
					}
					Connection.SaveChanges();

					LogIfAccessingAsDelegate(user, "deleted department " + department.Value.Name);
				}

				await FirstValidateAsCitizen(request, response)
					.NextEntityHoldsObserver(citizen, department, Connection, ValidationField.Create(nameof(request.Parameter.DepartmentId)))
					.NextEntityHoldsManager(citizen, superDepartment, Connection, ValidationField.Create(nameof(request.Parameter.DepartmentId)))
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public event ServiceEventHandler<ServiceEventArgs<IDepartmentEntity>> OnAdminRecruitedForAdmin;
		public event ServiceEventHandler<ServiceEventArgs<CitizenEntity>> OnAdminRecruitedForDepartment;
		public async Task<IResponse> RecruitAdminIntoDepartment(IAsCitizenEncryptableRequest<EditDepartmentAdminshipParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<SubDepartmentEntity> department = Connection.GetSingle<SubDepartmentEntity>(request.Parameter.DepartmentId);
				Lazy<IDepartmentEntity> superDepartment = Connection.GetFirst<IDepartmentEntity>(d => d.SubDepartments.Any(s => s.Id == department.Value.Id));
				Lazy<CitizenEntity> admin = Connection.GetSingle<CitizenEntity>(request.Parameter.AdminId);
				Lazy<CitizenSettingsEntity> settings = GetSettingsLazily<CitizenSettingsEntity, CitizenEntity>(admin);

				Boolean adminCheck()
				{
					return !admin.Value.HoldsAdminRight(Connection, department.Value);
				}
				Boolean canBeRecruitedCheck()
				{
					return settings.Value.CanBeRecruitedAsDepartmentAdmin;
				}
				void successAction()
				{
					GetService<IEventfulClaimService>().EnsureClaim(admin.Value, PBCommon.Configuration.Settings.ADMIN_RIGHT, department.Value);

					OnAdminRecruitedForAdmin.Invoke(Session, admin.Value, department.Value.CloneAsT());
					OnAdminRecruitedForDepartment.Invoke(Session, department.Value, admin.Value.CloneAsT());

					LogIfAccessingAsDelegate(user, "added admin {0} to department {1}", department.Value.Name, admin.Value.Name);
				}

				await FirstValidateAsCitizen(request, response)
					.NextEntityHoldsObserver(citizen, department, Connection, ValidationField.Create(nameof(request.Parameter.DepartmentId)))
					.NextEntityHoldsAdmin(citizen, superDepartment, Connection, ValidationField.Create(nameof(request.Parameter.DepartmentId)))
					.NextNullCheck(admin.Value,
						ValidationField.Create(nameof(request.Parameter.AdminId)),
						  ValidationCode.NotFound.WithMessage("The admin requested could not be found."))
					.NextCompound(adminCheck,
						ValidationField.Create(nameof(request.Parameter.AdminId)),
						ValidationCode.Duplicate.WithMessage("The admin requested has already been recruited into the department."))
					.NextCompound(canBeRecruitedCheck,
						ValidationCode.Invalid.WithMessage("The admin requested can not be recruited into this department."))
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public event ServiceEventHandler<ServiceEventArgs<IDepartmentEntity>> OnAdminResignedForAdmin;
		public event ServiceEventHandler<ServiceEventArgs<CitizenEntity>> OnAdminResignedForDepartment;
		public async Task<IResponse> ResignAdminFromDepartment(IAsCitizenEncryptableRequest<EditDepartmentAdminshipParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<SubDepartmentEntity> department = Connection.GetSingle<SubDepartmentEntity>(request.Parameter.DepartmentId);
				Lazy<IDepartmentEntity> superDepartment = Connection.GetFirst<IDepartmentEntity>(d => d.SubDepartments.Any(s => s.Id == department.Value.Id));
				Lazy<CitizenEntity> admin = new Lazy<CitizenEntity>(() => department.Value.GetAdminClaimsHolders<CitizenEntity>(Connection).SingleOrDefault(a => a.Id == request.Parameter.AdminId));

				void successAction()
				{
					GetService<IEventfulClaimService>().RemoveRight(admin.Value.GetHeldClaims(Connection, PBCommon.Configuration.Settings.ADMIN_RIGHT, department.Value).Single(), PBCommon.Configuration.Settings.ADMIN_RIGHT);

					OnAdminResignedForAdmin.Invoke(Session, admin.Value, department.Value.CloneAsT());
					OnAdminResignedForDepartment.Invoke(Session, department.Value, admin.Value.CloneAsT());

					LogIfAccessingAsDelegate(user, "removed admin {0} from department {1}", admin.Value.Name, department.Value.Name);
				}

				await FirstValidateAsCitizen(user, citizen, response.Validation)
					.NextEntityHoldsObserver(citizen, department, Connection, ValidationField.Create(nameof(request.Parameter.DepartmentId)))
					.NextEntityHoldsAdmin(citizen, superDepartment, Connection, ValidationField.Create(nameof(request.Parameter.DepartmentId)))
					.NextNullCheck(admin.Value,
						ValidationField.Create(nameof(request.Parameter.AdminId)),
						ValidationCode.NotFound.WithMessage("The admin requested could be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public event ServiceEventHandler<ServiceEventArgs<AccountEntityBase>> OnMemberRecruitedForDepartment;
		public event ServiceEventHandler<ServiceEventArgs<IDepartmentEntity>> OnMemberRecruitedForMember;
		public async Task<IResponse> RecruitMemberIntoDepartment(IAsCitizenEncryptableRequest<EditDepartmentMembershipParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<IDepartmentEntity> department = Connection.GetSingle<IDepartmentEntity>(request.Parameter.DepartmentId);
				Lazy<AccountEntityBase> member = Connection.GetSingle<AccountEntityBase>(request.Parameter.MemberId);
				Lazy<IAccountSettingsEntity> settings = Connection.GetFirst<IAccountSettingsEntity>(s => s.Owner.Id == department.Value.Id);

				Boolean memberCheck()
				{
					return !department.Value.Members.Any(m => m.Id == member.Value.Id);
				}
				Boolean canBeRecruitedCheck()
				{
					return settings.Value.CanBeRecruitedIntoDepartments;
				}
				void successAction()
				{
					department.Value.Members.Add(member.Value);
					Connection.Update(department);
					Connection.SaveChanges();

					OnMemberRecruitedForDepartment.Invoke(Session, department.Value, member.Value.CloneAsT());
					OnMemberRecruitedForMember.Invoke(Session, member.Value, department.Value.CloneAsT());

					LogIfAccessingAsDelegate(user, "added member " + member.Value.Name + " to department " + department.Value.Name);
				}

				await FirstValidateAsCitizen(user, citizen, response.Validation)
					.NextManagerManagesProperty(citizen, department, Connection, ValidationField.Create(nameof(request.Parameter.DepartmentId)))
					.NextObserverCanSeeProperty(citizen, member, Connection, ValidationField.Create(nameof(request.Parameter.MemberId)))
					.NextCompound(memberCheck,
						ValidationField.Create(nameof(request.Parameter.MemberId)),
						ValidationCode.Duplicate.WithMessage("The member requested has already been recruited into the departmment."))
					.NextCompound(canBeRecruitedCheck,
						ValidationCode.Invalid.WithMessage("The member requested can not be recruited into this department."))
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}
		public event ServiceEventHandler<ServiceEventArgs<AccountEntityBase>> OnMemberResignedForDepartment;
		public event ServiceEventHandler<ServiceEventArgs<IDepartmentEntity>> OnMemberResignedForMember;
		public async Task<IResponse> ResignMemberFromDepartment(IAsCitizenEncryptableRequest<EditDepartmentMembershipParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<IDepartmentEntity> department = Connection.GetSingle<IDepartmentEntity>(request.Parameter.DepartmentId);
				Lazy<AccountEntityBase> member = new Lazy<AccountEntityBase>(() => department.Value.Members.SingleOrDefault(m => m.Id == request.Parameter.MemberId));

				void successAction()
				{
					department.Value.Members.Remove(member.Value);
					Connection.Update(department.Value);
					Connection.SaveChanges();

					OnMemberResignedForDepartment.Invoke(Session, department.Value, member.Value.CloneAsT());
					OnMemberResignedForMember.Invoke(Session, member.Value, department.Value.CloneAsT());

					LogIfAccessingAsDelegate(user, "removed member " + member.Value.Name + " from department " + department.Value.Name);
				}

				await FirstValidateAsCitizen(user, citizen, response.Validation)
					.NextManagerManagesProperty(citizen, department, Connection, ValidationField.Create(nameof(request.Parameter.DepartmentId)))
					.NextNullCheck(member,
						ValidationField.Create(nameof(request.Parameter.MemberId)),
						ValidationCode.NotFound.WithMessage("The member requested could not be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public event ServiceEventHandler<ServiceEventArgs<TagEntity>> OnTagAdded;
		public async Task<IResponse> AddTagToDepartment(IAsCitizenEncryptableRequest<EditDepartmentTagsParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<IDepartmentEntity> department = Connection.GetSingle<IDepartmentEntity>(request.Parameter.DepartmentId);
				TagEntity tag = null;

				async Task<Boolean> tagsCheck()
				{
					tag ??= await GetService<IEventfulTagService>().GetTag(request.Parameter.TagName);
					return !department.Value.Tags.Any(m => m.Id == tag.Id);
				}
				void successAction()
				{
					department.Value.Tags.Add(tag);
					Connection.Update(department.Value);
					Connection.SaveChanges();

					OnTagAdded.Invoke(Session, department.Value, tag.CloneAsT());

					LogIfAccessingAsDelegate(user, "added tag " + tag.Name + " to department " + department.Value.Name);
				}

				await FirstValidateAsCitizen(user, citizen, response.Validation)
					.NextManagerManagesProperty(citizen, department, Connection, ValidationField.Create(nameof(request.Parameter.DepartmentId)))
					.NextCompound(tagsCheck,
						 ValidationField.Create(nameof(request.Parameter.TagName)),
						 ValidationCode.Duplicate.WithMessage("The tag requested has already been added to the department."))
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}
		public event ServiceEventHandler<ServiceEventArgs<TagEntity>> OnTagRemoved;
		public async Task<IResponse> RemoveTagFromDepartment(IAsCitizenEncryptableRequest<EditDepartmentTagsParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<IDepartmentEntity> department = Connection.GetSingle<IDepartmentEntity>(request.Parameter.DepartmentId);
				Lazy<TagEntity> tag = new Lazy<TagEntity>(() => department.Value.Tags.SingleOrDefault(t => t.Name.Equals(request.Parameter.TagName)));

				void successAction()
				{
					department.Value.Tags.Remove(tag.Value);
					Connection.Update(department);
					Connection.SaveChanges();

					OnTagRemoved.Invoke(Session, department.Value, tag.Value.CloneAsT());

					LogIfAccessingAsDelegate(user, "removed tag " + tag.Value.Name + " from department " + department.Value.Name);
				}

				await FirstValidateAsCitizen(user, citizen, response.Validation)
					.NextManagerManagesProperty(citizen, department, Connection, ValidationField.Create(nameof(request.Parameter.DepartmentId)))
					.NextNullCheck(tag,
						ValidationField.Create(nameof(request.Parameter.TagName)),
						ValidationCode.NotFound.WithMessage("The tag requested could not be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<IDepartmentEntity>> GetDepartmentsForMember(IAsAccountRequest request)
		{
			var response = new GetPaginatedEncryptableResponse<IDepartmentEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<IAccountEntity> account = GetAccountEntityLazily(request);

				void successAction()
				{
					response.Data = Connection.Query<IDepartmentEntity>()
						.Where(d => d.Members.Any(m => m.Id == account.Value.Id))
						.CloneAsT()
						.ToList();

					LogIfAccessingAsDelegate(user, "retrieved departments for member");
				}

				await FirstValidateAsCitizen(user, citizen, response.Validation)
					.NextManagerManagesProperty(citizen, account, Connection, ValidationField.Create(nameof(request.AsAccountId)))
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}
		public async Task<IGetPaginatedEncryptableResponse<IDepartmentEntity>> GetDepartmentsForAdmin(IAsCitizenRequest request)
		{
			var response = new GetPaginatedEncryptableResponse<IDepartmentEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);

				void successAction()
				{
					response.Data = Connection.Query<IDepartmentEntity>()
						.Where(d => d.Admins.Any(a => a.Id == citizen.Value.Id))
						.CloneAsT()
						.ToList();

					LogIfAccessingAsDelegate(user, "retrieved departments for admin");
				}

				await FirstValidateAsCitizen(user, citizen, response.Validation)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public event ServiceEventHandler<ServiceEventArgs<OnOrgsUpdatedData>> OnOrgsAdminsUpdated;
		public event ServiceEventHandler<ServiceEventArgs<OnOrgsUpdatedData>> OnOrgsMembersUpdated;
		public async Task<IResponse> UpdateOrgs(IAsAccountRequest request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<RealAccountEntity> asAccount = GetAccountEntityLazily<RealAccountEntity>(request);
				Lazy<CitizenEntity> asCitizen = new Lazy<CitizenEntity>(() => asAccount.Value.Owner);

				void successAction()
				{
					ScrapeX.Interfaces.IScraper scraper = new ScraperFactory()
					.CreateSinglePageScraper("https://robertsspaceindustries.com/citizens/" + asAccount.Value.Name + "/organizations");

					Dictionary<String, String> paths = new Dictionary<String, String>();

					List<IEntity> items = new List<IEntity>();

					List<OrgEntity> oldMemberIn = Connection.Query<OrgEntity>()
						.Where(o => o.Members.Any(m => m.Id == asAccount.Value.Id))
						.ToList();
					List<OrgEntity> newMemberIn = new List<OrgEntity>();


					List<OrgEntity> oldAdminIn = Connection.Query<OrgEntity>()
						.Where(o => o.Admins.Any(m => m.Id == asCitizen.Value.Id))
						.ToList();
					List<OrgEntity> newAdminIn = new List<OrgEntity>();

					for (Int32 i = 1; i < 11; i++)
					{
						StringBuilder basePath = new StringBuilder()
							.Append("/html/body/div[2]/div[2]/div[2]/div/div/div[2]/div[")
							.Append(i)
							.Append("]/div/div[2]/div/div[2]");

						StringBuilder getPath()
						{
							return new StringBuilder(basePath.ToString());
						}

						paths.Add("name" + i, getPath().Append("/p[1]/a").ToString());
						paths.Add("sid" + i, getPath().Append("/p[2]/strong").ToString());

						for (Int32 j = 1; j < 6; j++)
						{
							paths.Add("active" + i + "." + j, getPath()
								.Append("/div/span[@class=\"active\"][")
								.Append(j)
								.Append(']')
								.ToString());
						}
					}
					scraper.SetTargetPageXPaths(paths)
						   .Go((link, dict) =>
						   {
							   Boolean run = true;
							   for (Int32 i = 1; i < 11 && run; i++)
							   {
								   String name = dict["name" + i];
								   if (name == null || !name.IsValidOrgName())
								   {
									   run = false;
								   }
								   else
								   {

									   String sid = dict["sid" + i];

									   OrgEntity org = Connection.Query<OrgEntity>().Where(o => o.Name.Equals(name)).SingleOrDefault();

									   Int32 rank = dict.Skip(((i - 1) * 7) + 2).Take(5).ToList().Count(kvp => kvp.Value != null);


#if DEBUG
									   rank = 5;
#endif

									   void insertIntoDepartment()
									   {
										   if (rank == 5)
										   {
											   org.Creator = asCitizen.Value;
											   if (!oldAdminIn.Any(d => d.Id == org.Id))
											   {
												   org.Admins.Add(asCitizen.Value);
											   }
											   newAdminIn.Add(org);
										   }
										   if (!oldMemberIn.Any(d => d.Id == org.Id))
										   {
											   org.Members.Add(asAccount.Value);
										   }
										   newMemberIn.Add(org);
									   }

									   if (org == null)
									   {
										   org = new OrgEntity(name, sid);
										   insertIntoDepartment();
										   items.Add(org);
										   items.Add(new OrgSettingsEntity(org)
										   {
											   Accessibility = AccessibilityType.Public
										   });
									   }
									   else
									   {
										   insertIntoDepartment();
										   Connection.Update(org);
									   }
								   }
							   }
						   });

					items.ForEach(i => Connection.Insert(i));

					oldMemberIn
						.Where(o => !newMemberIn.Any(d => d.Id == o.Id))
						.ToList()
						.ForEach(o =>
						{
							o.Members.Remove(o.Members.Single(m => m.Id == asAccount.Value.Id));
							Connection.Update(o);
						});
					oldAdminIn
						.Where(o => !newAdminIn.Any(d => d.Id == o.Id))
						.ToList()
						.ForEach(o =>
						{
							o.Admins.Remove(o.Admins.Single(a => a.Id == asCitizen.Value.Id));
							Connection.Update(o);
						});

					Connection.SaveChanges();

					var adminIn = Connection.Query<OrgEntity>()
						.Where(o => o.Admins.Any(a => a.Id == asCitizen.Value.Id))
						.CloneAsT()
						.ToArray();

					OnOrgsAdminsUpdated.Invoke(Session, asCitizen.Value, new OnOrgsUpdatedData() { Orgs = adminIn });

					var memberIn = Connection.Query<OrgEntity>()
						.Where(o => o.Members.Any(m => m.Id == asAccount.Value.Id))
						.CloneAsT()
						.ToArray();

					OnOrgsMembersUpdated.Invoke(Session, asAccount.Value, new OnOrgsUpdatedData() { Orgs = memberIn });

					LogIfAccessingAsDelegate(user, "updated orgs");
				}

				await FirstValidateAsAccount(user, asCitizen, asAccount, response.Validation)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		private async Task<IEnumerable<TDepartment>> SearchDepartments<TDepartment, TDepartmentSettings, TParameter>(IAsAccountGetPaginatedEncryptableRequest<TParameter> request, IValidationFieldCollection validation)
			where TDepartment : IDepartmentEntity
			where TDepartmentSettings : IDepartmentSettingsEntity<TDepartment>
			where TParameter : SearchDepartmentsParameterBase
		{
			var retVal1 = Connection.Query<TDepartmentSettings>();
			if (request.Parameter.Accessibility.HasValue && request.Parameter.Accessibility.Value == AccessibilityType.Private)
			{
				var user = GetUserEntity(request);
				Boolean userIsInRoleCheck()
				{
					return user.IsInRole(PBCommon.Settings.ADMIN_ROLE) || user.IsInRole(PBCommon.Settings.SUPERADMIN_ROLE);
				}
				void accessibilitySuccessAction()
				{
					retVal1 = retVal1.Where(s => s.Accessibility == request.Parameter.Accessibility.Value);
				}

				await FirstValidateAsUser(user, validation)
					.NextCompound(userIsInRoleCheck)
					.SetOnCriterionMet(accessibilitySuccessAction)
					.Evaluate(response);
			}
			else
			{
				retVal1 = retVal1.Where(s => s.Accessibility == AccessibilityType.Public);
			}
			if (request.Parameter.InviteOnly.HasValue)
			{
				retVal1 = retVal1.Where(s => s.InviteOnly == request.Parameter.InviteOnly.Value);
			}
			var retVal2 = retVal1.Select(s => s.Owner);
			if (!String.IsNullOrWhiteSpace(request.Parameter.Name))
			{
				var name = request.Parameter.Name.Trim().ToLower();
				retVal2 = retVal2.Where(d => d.Name.ToLower().Contains(name));
			}
			if (request.Parameter.CreatorId.HasValue)
			{
				retVal2 = retVal2.Where(d => d.Creator.Id == request.Parameter.CreatorId.Value);
			}
			if (request.Parameter.ExcludeIds?.Any() ?? false)
			{
				retVal2 = retVal2.Where(d => !request.Parameter.ExcludeIds.Contains(d.Id));
			}
			if (request.Parameter.ExcludeNames?.Any() ?? false)
			{
				retVal2 = retVal2.Where(d => !request.Parameter.ExcludeNames.Contains(d.Name.ToLower()));
			}
			if (request.Parameter.TagsIds?.Any() ?? false)
			{
				retVal2 = retVal2.Where(d => !request.Parameter.TagsIds.All(id => d.Tags.Any(t => t.Id == id)));
			}
			if (request.Parameter.PriorityTagsIds?.Any() ?? false)
			{
				retVal2 = retVal2.Where(d => !request.Parameter.PriorityTagsIds.All(id => d.PriorityTags.Any(t => t.Id == id)));
			}
			if (request.Parameter.MembersIds?.Any() ?? false)
			{
				retVal2 = retVal2.Where(d => !request.Parameter.MembersIds.All(id => d.Members.Any(m => m.Id == id)));
			}
			if (request.Parameter.AdminsIds?.Any() ?? false)
			{
				retVal2 = retVal2.Where(d => !request.Parameter.AdminsIds.All(id => d.Admins.Any(a => a.Id == id)));
			}

			return retVal2;
		}

		public async Task<IGetPaginatedEncryptableResponse<IDepartmentEntity>> SearchDepartments(IAsAccountGetPaginatedEncryptableRequest<SearchDepartmentsParameterBase> request)
		{
			var response = new GetPaginatedEncryptableResponse<IDepartmentEntity>();

			async Task notNullRequest()
			{
				var data = await SearchDepartments<IDepartmentEntity, IDepartmentSettingsEntity<IDepartmentEntity>, SearchDepartmentsParameterBase>(request, response.Validation);

				void setData()
				{
					response.LastPage = data.GetPageCount(request.PerPage) - 1;
					response.Data = data.Paginate(request.PerPage, request.Page).Select(a => a.CloneAsT()).ToList();
				}
				await CachedCriterionChain.Cache.Get()
					.ThisValidatePagination(request, data, response.Validation)
					.SetOnCriterionMet(setData)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<OrgEntity>> SearchOrgs(IAsAccountGetPaginatedEncryptableRequest<SearchOrgsParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<OrgEntity>();

			async Task notNullRequest()
			{
				var data = await SearchDepartments<OrgEntity, OrgSettingsEntity, SearchOrgsParameter>(request, response.Validation);
				if (!String.IsNullOrWhiteSpace(request.Parameter.SID))
				{
					data = data.Where(d => String.Equals(request.Parameter.SID, d.SID));
				}

				void setData()
				{
					response.LastPage = data.GetPageCount(request.PerPage) - 1;
					response.Data = data.Paginate(request.PerPage, request.Page).Select(a => a.CloneAsT()).ToList();
				}
				await CachedCriterionChain.Cache.Get()
					.ThisValidatePagination(request, data, response.Validation)
					.SetOnCriterionMet(setData)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<SubDepartmentEntity>> SearchSubDepartments(IAsAccountGetPaginatedEncryptableRequest<SearchSubDepartmentsParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<SubDepartmentEntity>();

			async Task notNullRequest()
			{
				var data = await SearchDepartments<SubDepartmentEntity, SubDepartmentSettingsEntity, SearchSubDepartmentsParameter>(request, response.Validation);

				void setData()
				{
					response.LastPage = data.GetPageCount(request.PerPage) - 1;
					response.Data = data.Paginate(request.PerPage, request.Page).Select(a => a.CloneAsT()).ToList();
				}
				await CachedCriterionChain.Cache.Get()
					.ThisValidatePagination(request, data, response.Validation)
					.SetOnCriterionMet(setData)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}
	}
}
