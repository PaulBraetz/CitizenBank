

using CBApplication.Services.Abstractions;

using CBData.Entities;

using PBApplication.Events;
using PBApplication.Extensions;
using PBApplication.Requests.Abstractions;
using PBApplication.Responses;
using PBApplication.Responses.Abstractions;
using PBApplication.Services;
using PBApplication.Services.Abstractions;
using PBCommon.Validation;

using PBCommon.Extensions;
using PBCommon.Globalization;

using PBData.Entities;
using PBData.Extensions;

using System;
using System.Linq;

using static CBApplication.Services.Abstractions.IEventfulLogisticsService;
using static CBCommon.Enums.LogisticsEnums;
using System.Threading.Tasks;
using static CBApplication.Services.Abstractions.ILogisticsService;
using PBApplication.Context.Abstractions;
using PBCommon;

namespace CBApplication.Services
{
	public class LogisticsService : CBService, IEventfulLogisticsService
	{
		public LogisticsService(IServiceContext serviceContext) : base(serviceContext)
		{
			Observe<IEventfulLogisticsService>(this);
		}

		public event ServiceEventHandler<ServiceEventArgs<LogisticsOrderEntity>> OnLogisticsOrderCreated;
		public event ServiceEventHandler<ServiceEventArgs<LogisticsOrderEntity>> OnLogisticsOrderEdited;
		public event ServiceEventHandler<ServiceEventArgs> OnLogisticsOrderDeleted;

		public async Task<IResponse> CreateLogisticsOrder(IEventfulLogisticsService.CreateLogisticsOrderRequest request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				CitizenEntity citizen = Connection.GetSingle<CitizenEntity>(request.ClientId);

				async Task successAction()
				{

					Boolean targetCheck()
					{
						return !String.IsNullOrWhiteSpace(request.Target);
					}
					Boolean detailsCheck()
					{
						return !String.IsNullOrWhiteSpace(request.Details);
					}
					Boolean deadlineCheck()
					{
						return request.Deadline > TimeManager.Now;
					}
					void createOrder()
					{
						var newEntity = new LogisticsOrderEntity(request.Deadline, request.Target, citizen, request.Type, request.Details)
						{
							Origin = request.Origin
						};
						Connection.Insert(newEntity);
						Connection.SaveChanges();
						IQueryable<Guid> sessions = Connection.Query<UserSessionEntity>().Select(s => s.HubId);
						OnLogisticsOrderCreated.Invoke(Session, sessions, newEntity.CloneAsT());
					}

					var validChain = FirstCompound(targetCheck,
										response.Validation.GetField(nameof(request.Target)),
										DefaultCode.Invalid)
						.NextCompound(detailsCheck,
										response.Validation.GetField(nameof(request.Details)),
										DefaultCode.Invalid)
						.NextCompound(deadlineCheck,
										response.Validation.GetField(nameof(request.Deadline)),
										DefaultCode.Invalid)
						.SetOnCriterionMet(createOrder);

					async Task evaluate()
					{
						await validChain.Evaluate();
					}
					async Task successAction()
					{
						await FirstValidateAuthenticated(response.Validation)
							.NextManagerManagesProperty(Session.Owner, citizen.Owner, Connection, response.Validation.GetField(nameof(request.ClientId)))
							.SetOnCriterionMet(evaluate)
							.Evaluate();
					}

					await FirstNullCheck(citizen.Owner)
						.SetOnCriterionMet(successAction)
						.SetOnCriterionFailed(evaluate)
						.Evaluate();
				}

				await FirstNullCheck(citizen,
						response.Validation.GetField(nameof(request.ClientId)),
						DefaultCode.NotFound.SetMessage("The user requested could not be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IResponse> EditLogisticsOrder(IAsUserEncryptableRequest<EditLogisticsOrderParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				UserEntity user = GetUserEntity(request);
				Lazy<LogisticsOrderEntity> order = Connection.GetSingleLazily<LogisticsOrderEntity>(request.Parameter.LogisticsOrderId);

				Boolean roleCheck()
				{
					return user.IsInRole(CBCommon.Settings.Logistics.LOGISTICS_ROLE);
				}
				void successAction()
				{
					order.Value.Status = request.Parameter.Status;
					switch (order.Value.Status)
					{
						case OrderStatus.Underway:
							order.Value.ExpiryPaused = true;
							break;
						default:
							order.Value.RefreshNow();
							order.Value.ExpiryPaused = false;
							break;
					}

					Connection.Update(order.Value);
					Connection.SaveChanges();

					OnLogisticsOrderEdited.Invoke(Session, order.Value, order.Value.CloneAsT());
				}

				await FirstValidateAsUser(user, response.Validation)
					.NextCompound(roleCheck,
						response.Validation.GetField(nameof(request.AsUserId)),
						DefaultCode.Unauthorized.SetMessage("You are not authorized to edit logistics orders."))
					.NextNullCheck(order,
						response.Validation.GetField(nameof(request.Parameter.LogisticsOrderId)),
						DefaultCode.NotFound.SetMessage("The order requested could not be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<LogisticsOrderEntity>> GetLogisticsOrders(IGetPaginatedEncryptableRequest<GetLogisticsOrdersParameter> request)
		{
			var response = new GetPaginatedEncryptableResponse<LogisticsOrderEntity>();

			async Task notNullRequest()
			{
				var entities = Connection.Query<LogisticsOrderEntity>();
				void successAction()
				{
					void collect(LogisticsOrderEntity e)
					{
						OnLogisticsOrderDeleted.Invoke(e.CloneAsT());
						Connection.Delete(e);
					}
					GetService<IEventfulManageExpirantsService>().RunOverExpiredExpirants<LogisticsOrderEntity>(collect);
					Connection.SaveChanges();
					entities = entities.OrderBy(a => ((Int32)a.Status));
					response.LastPage = entities.GetPageCount(request.PerPage) - 1;
					response.Data = entities.Paginate(request.PerPage, request.Page)
											.Select(e => e.CloneAsT())
											.ToList();
				}

				await CachedCriterionChain.Cache.Get()
					.ThisValidatePagination(request, entities, response.Validation)
					.SetOnCriterionMet(successAction)
					.Evaluate();
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate();

			return response;
		}
	}
}
