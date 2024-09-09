using CBApplication.Requests.Abstractions;
using CBApplication.Services.Abstractions;

using CBData.Entities;

using static CBApplication.Services.Abstractions.ICBMessageService;

namespace CBApplication.Services
{
    public class CBMessageService : CBService, IEventfulCBMessageService
	{
		public CBMessageService(IServiceContext serviceContext) : base(serviceContext)
		{
			Observe<IEventfulCBMessageService>(this);
		}

		public async Task<IGetPaginatedEncryptableResponse<AccountMessageEntity>> GetAccountMessages(IAsAccountGetPaginatedRequest<GetAccountMessagesParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<AccountMessageEntity>();

			async Task notNullRequest()
			{
				async Task successAction()
				{
					var account = GetAccountEntity(request);

					var query = Connection.Query<AccountMessageEntity>()
						.Where(m => m.Recipients.Any(r => r.Id == account.Id));

					void setData()
					{
						UserEntity user = GetUserEntity(request);
						GetService<IEventfulManageExpirantsService>().DeleteExpirants<AccountMessageEntity>();
						response.Data = query
							.Paginate(request.PerPage, request.Page)
							.CloneAsT()
							.ToList();

						LogIfAccessingAsDelegate(user, "retrieved account messages for {0}", account.Name);
					}

					await CachedCriterionChain.Cache.Get()
						.ThisValidatePagination(request, query)
						.SetOnCriterionMet(setData)
						.Evaluate(response);
				}

				await FirstValidateAsAccount(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<CitizenMessageEntity>> GetCitizenMessages(IAsCitizenGetPaginatedRequest<GetCitizenMessagesParameter> request)
		{

			var response = new GetPaginatedEncryptableResponse<CitizenMessageEntity>();

			async Task notNullRequest()
			{
				async Task successAction()
				{
					var citizen = GetCitizenEntity(request);

					var query = Connection.Query<CitizenMessageEntity>()
						.Where(m => m.Recipients.Any(r => r.Id == citizen.Id));

					void setData()
					{
						UserEntity user = GetUserEntity(request);
						GetService<IEventfulManageExpirantsService>().DeleteExpirants<CitizenMessageEntity>();
						response.Data = query
							.Paginate(request.PerPage, request.Page)
							.CloneAsT()
							.ToList();

						LogIfAccessingAsDelegate(user, "retrieved citizen messages for {0}", citizen.Name);
					}

					await CachedCriterionChain.Cache.Get()
						.ThisValidatePagination(request, query)
						.SetOnCriterionMet(setData)
						.Evaluate(response);
				}

				await FirstValidateAsCitizen(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}


		public event ServiceEventHandler<ServiceEventArgs<CitizenMessageEntity>> OnCitizenMessageCreated;
		public void CreateCitizenMessages(CitizenEntity creator, IEnumerable<CitizenEntity> recipients, LocalizableFormattableString message)
		{

			CitizenMessageEntity newMessage = new(creator, recipients, message, TimeSpan.FromDays(3), true);
			Connection.Insert(newMessage);
			Connection.SaveChanges();

			List<CitizenEntity> eventRecipients = new() { creator };
			eventRecipients.AddRange(recipients);
			OnCitizenMessageCreated.Invoke(Session, eventRecipients, newMessage.CloneAsT());
		}
		public void CreateCitizenMessage(CitizenEntity creator, CitizenEntity recipient, LocalizableFormattableString message)
		{

			CreateCitizenMessages(creator, new[] { recipient }, message);
		}
		public void CreateCitizenSelfMessage(CitizenEntity creator, LocalizableFormattableString message)
		{

			CreateCitizenMessage(creator, creator, message);
		}

		public void CreateCitizenSelfMessages(IEnumerable<CitizenEntity> creators, LocalizableFormattableString message)
		{
			creators.ForEach(c => CreateCitizenSelfMessage(c, message));
		}

		public event ServiceEventHandler<ServiceEventArgs<AccountMessageEntity>> OnAccountMessageCreated;
		public void CreateAccountMessages(AccountEntityBase creator, IEnumerable<AccountEntityBase> recipients, LocalizableFormattableString message)
		{
			AccountMessageEntity newMessage = new(creator, recipients, message, TimeSpan.FromDays(3), false);
			Connection.Insert(newMessage);
			Connection.SaveChanges();

			List<AccountEntityBase> eventRecipients = new() { creator };
			eventRecipients.AddRange(recipients);
			OnAccountMessageCreated.Invoke(Session,
								  eventRecipients,
								  newMessage.CloneAsT());
		}
		public void CreateAccountMessage(AccountEntityBase creator, AccountEntityBase recipient, LocalizableFormattableString message)
		{
			CreateAccountMessages(creator, new[] { recipient }, message);
		}
		public void CreateAccountSelfMessage(AccountEntityBase creator, LocalizableFormattableString message)
		{
			CreateAccountMessage(creator, creator, message);
		}
		public void CreateAccountSelfMessages(IEnumerable<AccountEntityBase> creators, LocalizableFormattableString message)
		{
			creators.ForEach(c => CreateAccountSelfMessage(c, message));
		}
	}
}
