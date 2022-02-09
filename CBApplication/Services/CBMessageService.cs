using CBApplication.Requests;
using CBApplication.Requests.Abstractions;
using CBApplication.Services.Abstractions;

using CBData.Abstractions;
using CBData.Entities;
using PBApplication.Context.Abstractions;
using PBApplication.Events;
using PBApplication.Extensions;
using PBApplication.Responses;
using PBApplication.Responses.Abstractions;
using PBApplication.Services.Abstractions;
using PBCommon.Validation;
using PBData.Entities;
using PBData.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CBApplication.Services.Abstractions.IEventfulCBMessageService;
using static CBApplication.Services.Abstractions.ICBMessageService;
using PBCommon.Extensions;
using PBCommon;

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
			ConsoleLogger.Log(ConsoleLogger.Code.SRV, nameof(GetAccountMessages));

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

						LogIfAccessingAsDelegate(user, "retrieved account messages");
					}

					await CachedCriterionChain.Cache.Get()
						.ThisValidatePagination(request, query, response.Validation)
						.SetOnCriterionMet(setData)
						.Evaluate();
				}

				await FirstValidateAsAccount(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(response.Validation.GetField(nameof(request)))
				.Evaluate();

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<CitizenMessageEntity>> GetCitizenMessages(IAsCitizenGetPaginatedRequest<GetCitizenMessagesParameter> request)
		{
			ConsoleLogger.Log(ConsoleLogger.Code.SRV, nameof(GetCitizenMessages));

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

						LogIfAccessingAsDelegate(user, "retrieved citizen messages");
					}

					await CachedCriterionChain.Cache.Get()
						.ThisValidatePagination(request, query, response.Validation)
						.SetOnCriterionMet(setData)
						.Evaluate();
				}

				await FirstValidateAsCitizen(request, response)
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(response.Validation.GetField(nameof(request)))
				.Evaluate();

			return response;
		}


		public event ServiceEventHandler<ServiceEventArgs<CitizenMessageEntity>> OnCitizenMessageCreated;
		public void CreateCitizenMessages(CitizenEntity creator, ICollection<CitizenEntity> recipients, String message)
		{
			ConsoleLogger.Log(ConsoleLogger.Code.SRV, nameof(CreateCitizenMessages));

			CitizenMessageEntity newMessage = new CitizenMessageEntity(creator, recipients, message, TimeSpan.FromDays(3), true);
			Connection.Insert(newMessage);
			Connection.SaveChanges();

			List<CitizenEntity> eventRecipients = new List<CitizenEntity> { creator };
			eventRecipients.AddRange(recipients);
			OnCitizenMessageCreated.Invoke(Session, eventRecipients, newMessage.CloneAsT());
		}
		public void CreateCitizenMessage(CitizenEntity creator, CitizenEntity recipient, String message)
		{
			ConsoleLogger.Log(ConsoleLogger.Code.SRV, nameof(CreateCitizenMessage));

			CreateCitizenMessages(creator, new[] { recipient }, message);
		}
		public void CreateCitizenSelfMessage(CitizenEntity creator, String message)
		{
			ConsoleLogger.Log(ConsoleLogger.Code.SRV, nameof(CreateCitizenSelfMessage));

			CreateCitizenMessage(creator, creator, message);
		}
		public void CreateCitizenSelfMessages(ICollection<CitizenEntity> creators, String message)
		{
			ConsoleLogger.Log(ConsoleLogger.Code.SRV, nameof(CreateCitizenSelfMessages));

			creators.ForEach(c => CreateCitizenSelfMessage(c, message));
		}

		public event ServiceEventHandler<ServiceEventArgs<AccountMessageEntity>> OnAccountMessageCreated;
		public void CreateAccountMessages(AccountEntityBase creator, ICollection<AccountEntityBase> recipients, String message)
		{
			ConsoleLogger.Log(ConsoleLogger.Code.SRV, nameof(CreateAccountMessages));

			AccountMessageEntity newMessage = new AccountMessageEntity(creator, recipients, message, TimeSpan.FromDays(3), false);
			Connection.Insert(newMessage);
			Connection.SaveChanges();

			List<AccountEntityBase> eventRecipients = new List<AccountEntityBase> { creator };
			eventRecipients.AddRange(recipients);
			OnAccountMessageCreated.Invoke(Session,
								  eventRecipients,
								  newMessage.CloneAsT());
		}
		public void CreateAccountMessage(AccountEntityBase creator, AccountEntityBase recipient, String message)
		{
			ConsoleLogger.Log(ConsoleLogger.Code.SRV, nameof(CreateAccountMessage));

			CreateAccountMessages(creator, new[] { recipient }, message);
		}
		public void CreateAccountSelfMessage(AccountEntityBase creator, String message)
		{
			ConsoleLogger.Log(ConsoleLogger.Code.SRV, nameof(CreateAccountSelfMessage));

			CreateAccountMessage(creator, creator, message);
		}
		public void CreateAccountSelfMessages(ICollection<AccountEntityBase> creators, String message)
		{
			ConsoleLogger.Log(ConsoleLogger.Code.SRV, nameof(CreateAccountSelfMessages));

			creators.ForEach(c => CreateAccountSelfMessage(c, message));
		}
	}
}
