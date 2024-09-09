using CBApplication.Extensions;
using CBApplication.Requests.Abstractions;
using CBApplication.Services.Abstractions;

using CBCommon.Extensions;

using CBData.Abstractions;
using CBData.Entities;

using System.Text;

using static CBApplication.Services.Abstractions.IDepartmentService;
using static CBApplication.Services.Abstractions.IEventfulDepartmentService;

namespace CBApplication.Services
{
    public class DepartmentService : CBService, IEventfulDepartmentService
	{
		public DepartmentService(IServiceContext serviceContext) : base(serviceContext)
		{
			Observe<IEventfulDepartmentService>(this);
		}

		public event ServiceEventHandler<ServiceEventArgs<SubDepartmentEntity>> OnDepartmentCreated;
		public async Task<IResponse> CreateDepartment(IAsCitizenEncryptableRequest<CreateDepartmentParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				IDepartmentEntity superDepartment = Connection.GetSingle<IDepartmentEntity>(request.Parameter.SuperDepartmentId);
				CitizenEntity citizen = GetCitizenEntity(request);

				void successAction()
				{
					var newDepartment = new SubDepartmentEntity(citizen, request.Parameter.Name);
					var newSettings = new SubDepartmentSettingsEntity()
					{
						Accessibility = request.Parameter.Accessibility
					};
					Connection.Insert(newDepartment, newSettings);

					GetService<IEventfulClaimService>().EnsureClaim(newDepartment, newSettings, new IClaimService.EnsureRightDatum(PBCommon.Configuration.Settings.OwnerRight, true));

					superDepartment.SubDepartments.Add(newDepartment);
					Connection.Update(superDepartment);
					Connection.SaveChanges();

					var admins = superDepartment.GetAdminClaimsHolders<IEntity>(Connection);

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
				var citizen = GetCitizenEntity(request);
				var department = Connection.GetSingle<SubDepartmentEntity>(request.Parameter.DepartmentId);
				var superDepartment = Connection.GetFirst<IDepartmentEntity>(d => d.SubDepartments.Any(s => s.Id == department.Id));

				void successAction()
				{
					IOrderedEnumerable<SubDepartmentEntity> subDepartments = GetAllSubDepartments(department).OrderBy(d => d.CreationDate);
					foreach (SubDepartmentEntity subDepartment in subDepartments)
					{
						Connection.Delete(subDepartment);
						var messageService = GetService<IEventfulCBMessageService>();

						var members = subDepartment.GetClaimsHolders<AccountEntityBase>(Connection, CBCommon.Settings.CitizenBank.MEMBER_RIGHT);
						var admins = subDepartment.GetAdminClaimsHolders<CitizenEntity>(Connection);

						messageService.CreateAccountSelfMessages(members, new("{0} has deleted the subDepartmentartment {1}", citizen.Name, subDepartment.Name));
						messageService.CreateCitizenMessages(citizen, admins, new("I have deleted the subDepartmentartment {0}", subDepartment.Name));

						OnDepartmentDeleted.Invoke(subDepartment);
					}
					Connection.SaveChanges();

					var user = GetUserEntity(request);
					LogIfAccessingAsDelegate(user, "deleted department " + department.Name);
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
				var citizen = GetCitizenEntity(request);
				var department = Connection.GetSingle<SubDepartmentEntity>(request.Parameter.DepartmentId);
				Lazy<IDepartmentEntity> superDepartment = new(() => Connection.GetFirst<IDepartmentEntity>(d => d.SubDepartments.Any(s => s.Id == department.Id)));
				var admin = Connection.GetSingle<CitizenEntity>(request.Parameter.AdminId);
				Lazy<CitizenSettingsEntity> settings = new(() => GetSettings<CitizenSettingsEntity>(admin));

				Boolean adminCheck()
				{
					return !admin.HoldsAdminRight(Connection, department);
				}
				Boolean canBeRecruitedCheck()
				{
					return settings.Value.CanBeRecruitedAsDepartmentAdmin;
				}
				void successAction()
				{
					GetService<IEventfulClaimService>().EnsureClaim(admin, department, new IClaimService.EnsureRightDatum(PBCommon.Configuration.Settings.AdminRight, true));

					OnAdminRecruitedForAdmin.Invoke(Session, admin, department.CloneAsT());
					OnAdminRecruitedForDepartment.Invoke(Session, department, admin.CloneAsT());

					UserEntity user = GetUserEntity(request);
					LogIfAccessingAsDelegate(user, "added admin {0} to department {1}", department.Name, admin.Name);
				}

				await FirstValidateAsCitizen(request, response)
					.NextEntityHoldsObserver(citizen, department, Connection, ValidationField.Create(nameof(request.Parameter.DepartmentId)))
					.NextEntityHoldsAdmin(citizen, superDepartment.Value, Connection, ValidationField.Create(nameof(request.Parameter.DepartmentId)))
					.NextNullCheck(admin,
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
				var citizen = GetCitizenEntity(request);
				var department = Connection.GetSingle<SubDepartmentEntity>(request.Parameter.DepartmentId);
				Lazy<IDepartmentEntity> superDepartment = new(() => Connection.GetFirst<IDepartmentEntity>(d => d.SubDepartments.Any(s => s.Id == department.Id)));
				Lazy<CitizenEntity> admin = new(() => department.GetAdminClaimsHolders<CitizenEntity>(Connection).SingleOrDefault(a => a.Id == request.Parameter.AdminId));

				void successAction()
				{
					GetService<IEventfulClaimService>().EnsureClaim(admin.Value, department, new IClaimService.EnsureRightDatum(PBCommon.Configuration.Settings.AdminRight, false));

					OnAdminResignedForAdmin.Invoke(Session, admin.Value, department.CloneAsT());
					OnAdminResignedForDepartment.Invoke(Session, department, admin.Value.CloneAsT());

					UserEntity user = GetUserEntity(request);
					LogIfAccessingAsDelegate(user, "removed admin {0} from department {1}", admin.Value.Name, department.Name);
				}

				await FirstValidateAsCitizen(request, response)
					.NextEntityHoldsObserver(citizen, department, Connection, ValidationField.Create(nameof(request.Parameter.DepartmentId)))
					.NextEntityHoldsAdmin(citizen, superDepartment.Value, Connection, ValidationField.Create(nameof(request.Parameter.DepartmentId)))
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

		public event ServiceEventHandler<ServiceEventArgs<IAccountEntity>> OnMemberRecruitedForDepartment;
		public event ServiceEventHandler<ServiceEventArgs<IDepartmentEntity>> OnMemberRecruitedForMember;
		public async Task<IResponse> RecruitMemberIntoDepartment(IAsCitizenEncryptableRequest<EditDepartmentMembershipParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				var citizen = GetCitizenEntity(request);
				var department = Connection.GetSingle<IDepartmentEntity>(request.Parameter.DepartmentId);
				var member = Connection.GetSingle<IAccountEntity>(request.Parameter.MemberId);
				Lazy<IAccountSettingsEntity> settings = new(() => GetSettings<IAccountSettingsEntity>(department));

				Boolean memberCheck()
				{
					return !department.GetMemberClaimsHolders(Connection).Any(m => m.Id == member.Id);
				}
				Boolean canBeRecruitedCheck()
				{
					return settings.Value.CanBeRecruitedIntoDepartments;
				}
				void successAction()
				{
					GetService<IEventfulClaimService>().EnsureClaim(member, department, new IClaimService.EnsureRightDatum(CBCommon.Settings.CitizenBank.MEMBER_RIGHT, true));

					OnMemberRecruitedForDepartment.Invoke(Session, department, member.CloneAsT());
					OnMemberRecruitedForMember.Invoke(Session, member, department.CloneAsT());

					UserEntity user = GetUserEntity(request);
					LogIfAccessingAsDelegate(user, "added member " + member.Name + " to department " + department.Name);
				}

				await FirstValidateAsCitizen(request, response)
					.NextEntityHoldsManagerImplicitlyRecursively(citizen, department, Connection, ValidationField.Create(nameof(request.Parameter.DepartmentId)))
					.NextEntityHoldsObserverImplicitlyRecursively(citizen, member, Connection, ValidationField.Create(nameof(request.Parameter.MemberId)))
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
		public event ServiceEventHandler<ServiceEventArgs<IAccountEntity>> OnMemberResignedForDepartment;
		public event ServiceEventHandler<ServiceEventArgs<IDepartmentEntity>> OnMemberResignedForMember;
		public async Task<IResponse> ResignMemberFromDepartment(IAsCitizenEncryptableRequest<EditDepartmentMembershipParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				var citizen = GetCitizenEntity(request);
				var department = Connection.GetSingle<IDepartmentEntity>(request.Parameter.DepartmentId);
				Lazy<IAccountEntity> member = new(() => department.GetMemberClaimsHolders<IAccountEntity>(Connection).SingleOrDefault(m => m.Id == request.Parameter.MemberId));

				void successAction()
				{
					GetService<IEventfulClaimService>().EnsureClaim(member.Value, department, new IClaimService.EnsureRightDatum(CBCommon.Settings.CitizenBank.MEMBER_RIGHT, false));

					OnMemberResignedForDepartment.Invoke(Session, department, member.Value.CloneAsT());
					OnMemberResignedForMember.Invoke(Session, member.Value, department.CloneAsT());

					UserEntity user = GetUserEntity(request);
					LogIfAccessingAsDelegate(user, "removed member " + member.Value.Name + " from department " + department.Name);
				}

				await FirstValidateAsCitizen(request, response)
					.NextEntityHoldsManagerImplicitlyRecursively(citizen, department, Connection, ValidationField.Create(nameof(request.Parameter.DepartmentId)))
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
				var citizen = GetCitizenEntity(request);
				var department = Connection.GetSingle<IDepartmentEntity>(request.Parameter.DepartmentId);
				TagEntity tag = null;

				async Task<Boolean> tagsCheck()
				{
					tag ??= await GetService<IEventfulTagService>().GetTag(request.Parameter.TagName);
					return !department.Tags.Any(m => m.Id == tag.Id);
				}
				void successAction()
				{
					department.Tags.Add(tag);
					Connection.Update(department);
					Connection.SaveChanges();

					OnTagAdded.Invoke(Session, department, tag.CloneAsT());

					UserEntity user = GetUserEntity(request);
					LogIfAccessingAsDelegate(user, "added tag " + tag.Name + " to department " + department.Name);
				}

				await FirstValidateAsCitizen(request, response)
					.NextEntityHoldsManagerImplicitlyRecursively(citizen, department, Connection, ValidationField.Create(nameof(request.Parameter.DepartmentId)))
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
				var citizen = GetCitizenEntity(request);
				var department = Connection.GetSingle<IDepartmentEntity>(request.Parameter.DepartmentId);
				Lazy<TagEntity> tag = new(() => department.Tags.SingleOrDefault(t => t.Name.Equals(request.Parameter.TagName)));

				void successAction()
				{
					department.Tags.Remove(tag.Value);
					Connection.Update(department);
					Connection.SaveChanges();

					OnTagRemoved.Invoke(Session, department, tag.Value.CloneAsT());

					UserEntity user = GetUserEntity(request);
					LogIfAccessingAsDelegate(user, "removed tag " + tag.Value.Name + " from department " + department.Name);
				}

				await FirstValidateAsCitizen(request, response)
					.NextEntityHoldsManagerImplicitlyRecursively(citizen,
						department,
						Connection,
						ValidationField.Create(nameof(request.Parameter.DepartmentId)))
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
				var citizen = GetCitizenEntity(request);
				var account = GetAccountEntity<IAccountEntity>(request);

				void successAction()
				{
					response.Data = account.GetHeldClaimsValues<IDepartmentEntity>(Connection, CBCommon.Settings.CitizenBank.MEMBER_RIGHT)
						.CloneAsT()
						.ToList();

					UserEntity user = GetUserEntity(request);
					LogIfAccessingAsDelegate(user, "retrieved departments for member");
				}

				await FirstValidateAsCitizen(request, response)
					.NextEntityHoldsManagerImplicitlyRecursively(citizen, account, Connection, ValidationField.Create(nameof(request.AsAccountId)))
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
				var citizen = GetCitizenEntity(request);

				void successAction()
				{
					response.Data = citizen.GetHeldAdminClaimsValues<IDepartmentEntity>(Connection)
						.CloneAsT()
						.ToList();

					UserEntity user = GetUserEntity(request);
					LogIfAccessingAsDelegate(user, "retrieved departments for admin");
				}

				await FirstValidateAsCitizen(request, response)
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
				void successAction()
				{
					var account = GetAccountEntity<RealAccountEntity>(request);
					Lazy<CitizenEntity> asCitizen = new(() => account.GetOwnerClaimsHolders<CitizenEntity>(Connection).Single());

					ScrapeX.Interfaces.IScraper scraper = new ScraperFactory()
					.CreateSinglePageScraper("https://robertsspaceindustries.com/citizens/" + account.Name + "/organizations");

					Dictionary<String, String> paths = new();

					var items = new List<IEntity>();

					var oldMemberIn = account.GetHeldClaimsValues<OrgEntity>(Connection, CBCommon.Settings.CitizenBank.MEMBER_RIGHT).ToList();
					var newMemberIn = new List<OrgEntity>();


					var oldAdminIn = account.GetHeldAdminClaimsValues<OrgEntity>(Connection).ToList();
					var newAdminIn = new List<OrgEntity>();

					var claimService = GetService<IEventfulClaimService>();

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

										 void insertIntoDepartment()
										 {
											 if (rank == 5)
											 {
												 org.Creator = asCitizen.Value;
												 if (!oldAdminIn.Any(d => d.Id == org.Id))
												 {
													 GetService<IEventfulClaimService>().EnsureClaim(asCitizen.Value, org, new IClaimService.EnsureRightDatum(PBCommon.Configuration.Settings.AdminRight, true));
												 }
												 newAdminIn.Add(org);
											 }
											 if (!oldMemberIn.Any(d => d.Id == org.Id))
											 {
												 GetService<IEventfulClaimService>().EnsureClaim(account, org, new IClaimService.EnsureRightDatum(CBCommon.Settings.CitizenBank.MEMBER_RIGHT, true));
											 }
											 newMemberIn.Add(org);
										 }

										 if (org == null)
										 {
											 org = new OrgEntity(name, sid);
											 insertIntoDepartment();
											 var settings = new OrgSettingsEntity()
											 {
												 Accessibility = AccessibilityType.Public
											 };
											 Connection.Insert(org);
											 Connection.Insert(settings);

											 //TODO: settings ownership is immutable => use property
											 GetService<IEventfulClaimService>().EnsureClaim(org, settings, new IClaimService.EnsureRightDatum(PBCommon.Configuration.Settings.OwnerRight, true));
										 }
										 else
										 {
											 insertIntoDepartment();
											 Connection.Update(org);
										 }
									 }
								 }
							 });

					Connection.Insert(items);

					oldMemberIn
						.Where(o => !newMemberIn.Any(d => d.Id == o.Id))
						.ForEach(o =>
						{
							GetService<IEventfulClaimService>().EnsureClaim(account, o, new IClaimService.EnsureRightDatum(CBCommon.Settings.CitizenBank.MEMBER_RIGHT, false));
						});
					oldAdminIn
						.Where(o => !newAdminIn.Any(d => d.Id == o.Id))
						.ForEach(o =>
						{
							GetService<IEventfulClaimService>().EnsureClaim(account, o, new IClaimService.EnsureRightDatum(PBCommon.Configuration.Settings.AdminRight, false));
						});

					Connection.SaveChanges();

					var adminIn = asCitizen.Value.GetHeldAdminClaimsValues<OrgEntity>(Connection).CloneAsT().ToList();

					OnOrgsAdminsUpdated.Invoke(Session, asCitizen.Value, new OnOrgsUpdatedData() { Orgs = adminIn });

					var memberIn = asCitizen.Value.GetHeldClaimsValues<OrgEntity>(Connection, CBCommon.Settings.CitizenBank.MEMBER_RIGHT)
						.CloneAsT()
						.ToArray();

					OnOrgsMembersUpdated.Invoke(Session, account, new OnOrgsUpdatedData() { Orgs = memberIn });

					UserEntity user = GetUserEntity(request);
					LogIfAccessingAsDelegate(user, "updated orgs");
				}

				await FirstValidateAsAccount(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}

		private async Task<IEnumerable<(TDepartment Department, TDepartmentSettings Settings)>> SearchDepartments<TDepartment, TDepartmentSettings, TParameter>(IAsAccountGetPaginatedEncryptableRequest<TParameter> request, IResponse response)
			where TDepartment : IDepartmentEntity
			where TDepartmentSettings : IDepartmentSettingsEntity
			where TParameter : SearchDepartmentsParameterBase
		{
			//Sort by account first, since most common search parameter will likely be name
			//Since results have to be instanciated (.ToList()), most of the result set should have been filtered beforehand, so as not to query too many datasets.

			IEnumerable<TDepartment> retVal1 = Connection.Query<TDepartment>();
			if (!String.IsNullOrWhiteSpace(request.Parameter.Name))
			{
				var name = request.Parameter.Name.Trim().ToLower();
				retVal1 = retVal1.Where(d => d.Name.ToLower().Contains(name));
			}
			if (request.Parameter.CreatorId.HasValue)
			{
				retVal1 = retVal1.Where(d => d.Creator.Id == request.Parameter.CreatorId.Value);
			}
			if (request.Parameter.ExcludeIds?.Any() ?? false)
			{
				retVal1 = retVal1.Where(d => !request.Parameter.ExcludeIds.Contains(d.Id));
			}
			if (request.Parameter.ExcludeNames?.Any() ?? false)
			{
				retVal1 = retVal1.Where(d => !request.Parameter.ExcludeNames.Contains(d.Name.ToLower()));
			}
			if (request.Parameter.TagsIds?.Any() ?? false)
			{
				retVal1 = retVal1.Where(d => !request.Parameter.TagsIds.All(id => d.Tags.Any(t => t.Id == id)));
			}
			if (request.Parameter.PriorityTagsIds?.Any() ?? false)
			{
				retVal1 = retVal1.Where(d => !request.Parameter.PriorityTagsIds.All(id => d.PriorityTags.Any(t => t.Id == id)));
			}

			retVal1 = retVal1.ToList();

			var retVal2 = retVal1.Select(d => (Department: d, Settings: GetSettings<TDepartmentSettings>(d)));

			if (request.Parameter.Accessibility.HasValue && request.Parameter.Accessibility.Value == AccessibilityType.Private)
			{
				var user = GetUserEntity(request);
				Boolean userIsInRoleCheck()
				{
					return user.HoldsAdminRight(Connection) || user.HoldsOwnerRight(Connection);
				}
				void isAdminOrOwner()
				{
					retVal2 = retVal2.Where(t => t.Settings.Accessibility == request.Parameter.Accessibility.Value);
				}
				void isNeitherAdminNorOwner()
				{
					retVal2 = retVal2.Where(t => t.Settings.Accessibility == request.Parameter.Accessibility.Value && (user.HoldsOwnerRightRecursively(Connection, t.Department) || user.HoldsAdminRightRecursively(Connection, t.Department) || user.HoldsObserverRightRecursively(Connection, t.Department)));
				}

				await FirstValidateAuthenticatedDelegate(request, response)
					.NextCompound(userIsInRoleCheck)
					.SetOnCriterionMet(isAdminOrOwner)
					.SetOnCriterionFailed(isNeitherAdminNorOwner)
					.Evaluate(response);
			}
			else
			{
				retVal2 = retVal2.Where(t => t.Settings.Accessibility == AccessibilityType.Public);
			}
			if (request.Parameter.InviteOnly.HasValue)
			{
				retVal2 = retVal2.Where(t => t.Settings.InviteOnly == request.Parameter.InviteOnly.Value);
			}

			return retVal2;
		}

		public async Task<IGetPaginatedEncryptableResponse<OrgEntity>> SearchOrgs(IAsAccountGetPaginatedEncryptableRequest<SearchOrgsParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<OrgEntity>();

			async Task notNullRequest()
			{
				var data = await SearchDepartments<OrgEntity, OrgSettingsEntity, SearchOrgsParameter>(request, response);
				if (!String.IsNullOrWhiteSpace(request.Parameter.SID))
				{
					data = data.Where(t => String.Equals(request.Parameter.SID, t.Department.SID));
				}

				void setData()
				{
					response.LastPage = data.GetPageCount(request.PerPage) - 1;
					response.Data = data.Paginate(request.PerPage, request.Page)
						.Select(t => t.Department)
						.CloneAsT()
						.ToList();
				}
				await CachedCriterionChain.Cache.Get()
					.ThisValidatePagination(request, data)
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
				var data = await SearchDepartments<SubDepartmentEntity, SubDepartmentSettingsEntity, SearchSubDepartmentsParameter>(request, response);

				void setData()
				{
					response.LastPage = data.GetPageCount(request.PerPage) - 1;
					response.Data = data.Paginate(request.PerPage, request.Page)
						.Select(t => t.Department)
						.CloneAsT()
						.ToList();
				}
				await CachedCriterionChain.Cache.Get()
					.ThisValidatePagination(request, data)
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
