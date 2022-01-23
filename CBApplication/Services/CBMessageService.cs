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

namespace CBApplication.Services
{
	public class CBMessageService : CBService, IEventfulCBMessageService
	{
		public CBMessageService(IServiceContext serviceContext) : base(serviceContext)
		{
			Observe<IEventfulCBMessageService>(this);
		}

		public async Task<IGetPaginatedEncryptableResponse<AccountMessageEntity>> GetAccountMessages(IGetPaginatedAsAccountRequest<GetAccountMessagesParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<AccountMessageEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
				Lazy<IAccountEntity> account = GetAccountEntityLazily(request);

				async Task successAction()
				{
					var query = Connection.Query<AccountMessageEntity>()
						.Where(m => m.Recipients.Any(r => r.Id == user.Id));

					void setData()
					{
						UserEntity user = GetUserEntity(request);
						GetService<IEventfulManageExpirantsService>().DeleteExpirants<AccountMessageEntity>();
						response.Data = query
							.Select(m => m.CloneAsT())
							.ToList();

						LogIfAccessingAsDelegate(user, "retrieved account messages");
					}

					await CachedCriterionChain.Cache.Get()
						.ThisValidatePagination(request, query, response.Validation)
						.SetOnCriterionMet(setData)
						.Evaluate();
				}

				await FirstValidateAsAccount(user, citizen, account, response.Validation)
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<CitizenMessageEntity>> GetCitizenMessages(IGetPaginatedAsCitizenRequest<GetCitizenMessagesParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<CitizenMessageEntity>();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);

				async Task successAction()
				{
					var query = Connection.Query<CitizenMessageEntity>()
						.Where(m => m.Recipients.Any(r => r.Id == user.Id));

					void setData()
					{
						UserEntity user = GetUserEntity(request);
						GetService<IEventfulManageExpirantsService>().DeleteExpirants<CitizenMessageEntity>();
						response.Data = query
							.Select(m => m.CloneAsT())
							.ToList();

						LogIfAccessingAsDelegate(user, "retrieved citizen messages");
					}

					await CachedCriterionChain.Cache.Get()
						.ThisValidatePagination(request, query, response.Validation)
						.SetOnCriterionMet(setData)
						.Evaluate();
				}

				await FirstValidateAsCitizen(user, citizen, response.Validation)
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}


		public event ServiceEventHandler<ServiceEventArgs<CitizenMessageEntity>> OnCitizenMessageCreated;
		public void CreateCitizenMessages(CitizenEntity creator, ICollection<CitizenEntity> recipients, String message)
		{
			CitizenMessageEntity newMessage = new CitizenMessageEntity(creator, message, TimeSpan.FromDays(3), false)
			{
				Recipients = recipients
			};
			Connection.Insert(newMessage);
			Connection.SaveChanges();

			List<CitizenEntity> eventRecipients = new List<CitizenEntity> { creator };
			eventRecipients.AddRange(recipients);
			OnCitizenMessageCreated.Invoke(Session,
								  eventRecipients,
								 newMessage.CloneAsT());
		}
		public void CreateCitizenMessage(CitizenEntity creator, CitizenEntity recipient, String message)
		{
			CreateCitizenMessages(creator, new List<CitizenEntity> { recipient }, message);
		}
		public void CreateCitizenSelfMessage(CitizenEntity creator, String message)
		{
			CreateCitizenMessage(creator, creator, message);
		}
		public void CreateCitizenSelfMessages(ICollection<CitizenEntity> creators, String message)
		{
			creators.ForEach(c => CreateCitizenSelfMessage(c, message));
		}

		public event ServiceEventHandler<ServiceEventArgs<AccountMessageEntity>> OnAccountMessageCreated;
		public void CreateAccountMessages(AccountEntityBase creator, ICollection<AccountEntityBase> recipients, String message)
		{
			AccountMessageEntity newMessage = new AccountMessageEntity(creator, message, TimeSpan.FromDays(3), false)
			{
				Recipients = recipients
			};
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
			CreateAccountMessages(creator, new List<AccountEntityBase> { recipient }, message);
		}
		public void CreateAccountSelfMessage(AccountEntityBase creator, String message)
		{
			CreateAccountMessage(creator, creator, message);
		}
		public void CreateAccountSelfMessages(ICollection<AccountEntityBase> creators, String message)
		{
			creators.ForEach(c => CreateAccountSelfMessage(c, message));
		}
	}
}
