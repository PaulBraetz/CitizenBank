

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

using static CBCommon.Enums.LogisticsEnums;
using System.Threading.Tasks;
using static CBApplication.Services.Abstractions.ILogisticsService;
using PBApplication.Context.Abstractions;
using PBCommon;
using CBApplication.Requests.Abstractions;

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

		public async Task<IResponse> CreateLogisticsOrder(IAsCitizenRequest<CreateLogisticsOrderParameter> request)
		{
			ConsoleLogger.Log(ConsoleLogger.Code.SRV, nameof(CreateLogisticsOrder));

			var response = new Response();

			async Task notNullRequest()
			{
				async Task successAction()
				{
					var client = GetCitizenEntity(request);

					Boolean targetCheck()
					{
						return !String.IsNullOrWhiteSpace(request.Parameter.Target);
					}
					Boolean detailsCheck()
					{
						return !String.IsNullOrWhiteSpace(request.Parameter.Details);
					}
					Boolean deadlineCheck()
					{
						return request.Parameter.Deadline > TimeManager.Now;
					}
					void createOrder()
					{
						var newEntity = new LogisticsOrderEntity(request.Parameter.Deadline, request.Parameter.Target, client, request.Parameter.Type, request.Parameter.Details)
						{
							Origin = request.Parameter.Origin
						};
						Connection.Insert(newEntity);
						Connection.SaveChanges();
						IQueryable<Guid> sessions = Connection.Query<UserSessionEntity>().Select(s => s.HubId);
						OnLogisticsOrderCreated.Invoke(Session, sessions, newEntity.CloneAsT());
					}

					await FirstCompound(targetCheck,
										response.Validation.GetField(nameof(request.Parameter.Target)),
										DefaultCode.Invalid)
						.NextCompound(detailsCheck,
										response.Validation.GetField(nameof(request.Parameter.Details)),
										DefaultCode.Invalid)
						.NextCompound(deadlineCheck,
										response.Validation.GetField(nameof(request.Parameter.Deadline)),
										DefaultCode.Invalid)
						.SetOnCriterionMet(createOrder)
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

		public async Task<IResponse> EditLogisticsOrder(IAsCitizenEncryptableRequest<EditLogisticsOrderParameter> request)
		{
			ConsoleLogger.Log(ConsoleLogger.Code.SRV, nameof(EditLogisticsOrder));

			var response = new Response();

			async Task notNullRequest()
			{
				var manager = GetCitizenEntityLazily(request);
				Lazy<LogisticsOrderEntity> order = Connection.GetSingleLazily<LogisticsOrderEntity>(request.Parameter.LogisticsOrderId);

				Boolean roleCheck()
				{
					return manager.Value.HoldsClaim(Connection, CBCommon.Settings.Logistics.LOGISTICS_MANAGER_RIGHT);
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

				await FirstValidateAsCitizen(request, response)
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
				.CatchAll(response.Validation.GetField(nameof(request)))
				.Evaluate();

			return response;
		}

		public async Task<IGetPaginatedEncryptableResponse<LogisticsOrderEntity>> GetLogisticsOrders(IGetPaginatedEncryptableRequest<GetLogisticsOrdersParameter> request)
		{
			ConsoleLogger.Log(ConsoleLogger.Code.SRV, nameof(GetLogisticsOrders));

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
