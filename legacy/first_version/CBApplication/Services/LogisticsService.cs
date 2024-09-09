

using CBApplication.Requests.Abstractions;
using CBApplication.Services.Abstractions;

using CBData.Entities;

using static CBApplication.Services.Abstractions.ILogisticsService;
using static CBCommon.Enums.LogisticsEnums;

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
						var orderId = GetService<IEventfulCUDService>().GenerateUniqueVerification<LogisticsOrderEntity>();
						var newEntity = new LogisticsOrderEntity(request.Parameter.Deadline, request.Parameter.Target, client, request.Parameter.Type, request.Parameter.Details, orderId)
						{
							Origin = request.Parameter.Origin
						};
						Connection.Insert(newEntity);
						Connection.SaveChanges();
						IQueryable<Guid> sessions = Connection.Query<UserSessionEntity>().Select(s => s.HubId);
						OnLogisticsOrderCreated.Invoke(Session, sessions, newEntity.CloneAsT());
					}

					await FirstCompound(targetCheck,
										ValidationField.Create(nameof(request.Parameter.Target)),
										ValidationCode.Invalid)
						.NextCompound(detailsCheck,
										ValidationField.Create(nameof(request.Parameter.Details)),
										ValidationCode.Invalid)
						.NextCompound(deadlineCheck,
										ValidationField.Create(nameof(request.Parameter.Deadline)),
										ValidationCode.Invalid)
						.SetOnCriterionMet(createOrder)
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

		public async Task<IResponse> EditLogisticsOrder(IAsCitizenEncryptableRequest<EditLogisticsOrderParameter> request)
		{
			var response = new Response();

			async Task notNullRequest()
			{
				var manager = GetCitizenEntity(request);
				var order = Connection.GetSingle<LogisticsOrderEntity>(request.Parameter.LogisticsOrderId);

				void successAction()
				{
					order.Status = request.Parameter.Status;
					switch (order.Status)
					{
						case OrderStatus.Underway:
							order.ExpiryPaused = true;
							break;
						default:
							order.RefreshNow();
							order.ExpiryPaused = false;
							break;
					}

					Connection.Update(order);
					Connection.SaveChanges();

					OnLogisticsOrderEdited.Invoke(Session, order, order.CloneAsT());
				}

				await FirstValidateAsCitizen(request, response)
					.NextEntityHoldsClaim(manager,
						CBCommon.Settings.Logistics.LOGISTICS_MANAGER_RIGHT,
						Connection,
						ValidationField.Create(nameof(request.AsUserId)),
						ValidationCode.Unauthorized.WithMessage("You are not authorized to edit logistics orders."))
					.NextNullCheck(order,
						ValidationField.Create(nameof(request.Parameter.LogisticsOrderId)),
						ValidationCode.NotFound.WithMessage("The order requested could not be found."))
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

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
					.ThisValidatePagination(request, entities)
					.SetOnCriterionMet(successAction)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.Evaluate(response);

			return response;
		}
	}
}
