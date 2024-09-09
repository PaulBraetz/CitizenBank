using CBApplication.Services;
using CBApplication.Services.Abstractions;

using CitizenBank.Events.Observers;

namespace CitizenBank.Events
{
    public class CBObservingServiceContext : ObservingServiceContext
	{
		public CBObservingServiceContext(IHttpContextAccessor httpContextAccessor, IHubContext<EventHub> hubContext, ISerializer serializer) : base(httpContextAccessor, hubContext, serializer)
		{
			RegisterTypeToServices<IEventfulAccountService, AccountService>(new InjectionConstructor(this));
			RegisterTypeToServices<IEventfulCBMessageService, CBMessageService>(new InjectionConstructor(this));
			RegisterTypeToServices<IEventfulCitizenService, CitizenService>(new InjectionConstructor(this));
			RegisterTypeToServices<IEventfulDepartmentService, DepartmentService>(new InjectionConstructor(this));
			RegisterTypeToServices<IEventfulTransactionService, TransactionService>(new InjectionConstructor(this));
			RegisterTypeToServices<IEventfulLogisticsService, LogisticsService>(new InjectionConstructor(this));

			RegisterTypeToServices<ICitizenService, CitizenService>(new InjectionConstructor(this));
			RegisterTypeToServices<IDepartmentService, DepartmentService>(new InjectionConstructor(this));

			RegisterTypeToServices<IAccountService, AccountService>(new InjectionConstructor(this));
			RegisterTypeToServices<ICBMessageService, CBMessageService>(new InjectionConstructor(this));
			RegisterTypeToServices<IService, CitizenService>(new InjectionConstructor(this));
			RegisterTypeToServices<ITransactionService, TransactionService>(new InjectionConstructor(this));
			RegisterTypeToServices<ILogisticsService, LogisticsService>(new InjectionConstructor(this));

			IEncryptionService encryptionService = GetService<IEncryptionService>();

			RegisterTypeToObservers<IServiceObserver<IEventfulCitizenService>, CitizenServiceObserver, IEventfulCitizenService>(new InjectionConstructor(hubContext, encryptionService, serializer));
			RegisterTypeToObservers<IServiceObserver<IEventfulLogisticsService>, LogisticsServiceObserver, IEventfulLogisticsService>(new InjectionConstructor(hubContext, encryptionService, serializer));
			RegisterTypeToObservers<IServiceObserver<IEventfulAccountService>, AccountServiceObserver, IEventfulAccountService>(new InjectionConstructor(hubContext, encryptionService, serializer));
		}
	}
}
