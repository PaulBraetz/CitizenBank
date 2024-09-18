namespace Tests.Integration;

using CitizenBank.Composition;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;

using RhoMicro.ApplicationFramework.Composition;

using SimpleInjector;
using SimpleInjector.Lifestyles;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public abstract class IntegrationTestBase
{
    protected readonly struct ServiceScope<TService>(TService service, IEnumerable<IDisposable> disposables) : IDisposable
    {
        public TService Service { get; } = service;

        public void Dispose()
        {
            foreach(var d in disposables)
                d.Dispose();
        }
    }
    static Int32 _dbIndex;
    protected ServiceScope<TService> CreateService<TService>(params (Type serviceType, Object instance)[] mockRegistrations)
        where TService : class
    {
        var container = new Container();
        container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;
        container.Options.EnableAutoVerification = false;

        var connectionString = $"Data Source=TestDb_{Interlocked.Increment(ref _dbIndex)};Mode=Memory;Cache=Shared";

        _ = new ServiceCollection()
            .AddSimpleInjector(container, o =>
            {
                _ = o.Services
                    .AddConfiguration(new ConfigurationBuilder()
                        .AddInMemoryCollection(new Dictionary<String, String?>()
                        {
                            ["ConnectionStrings:CitizenBankContext"] = connectionString,
                            ["DoesCitizenExistSettings:QueryUrlFormat"] = "{0}",
                            ["LoadBioSettings:QueryUrlFormat"] = "{0}"
                        }));

                CoreComposers.SimpleinjectorAddHandler.Invoke(o);
                ClientsideComposers.SimpleinjectorAddHandler.Invoke(o);
                ServersideComposers.SimpleinjectorAddHandler.Invoke(o);
            })
            .BuildServiceProvider()
            .UseSimpleInjector(container);

        var mocksMap = mockRegistrations.ToDictionary(t => t.serviceType, t => t.instance);
        var composer =
            PresentationComposers.Models +
            CoreComposers.Instance +
            ClientsideComposers.LocalClient +
            Composer.Create(c =>
            {
                c.RegisterServices(
                    ConventionalServiceRegistrationOptions.PreferAssembly(typeof(ServersideComposers).Assembly) with
                    {
                        RegistrationPredicate = info =>
                            ConventionalServiceRegistrationPredicates.IgnoreAttributeFakes.Invoke(info)
                            && !mocksMap.ContainsKey(info.TraditionalServiceType)
                    },
                    typeof(CoreComposers).Assembly,
                    typeof(ClientsideComposers).Assembly,
                    typeof(ServersideComposers).Assembly);

                c.Register<IJSRuntime, FakeJsRuntime>(Lifestyle.Singleton);
                c.RegisterInstance<ILogger>(NullLogger.Instance);
                foreach(var (service, instance) in mocksMap)
                    c.RegisterInstance(service, instance);
            });

        composer.Compose(container);

        var connection = new SqliteConnection(connectionString);
        connection.Open();

        container.Verify(VerificationOption.VerifyAndDiagnose);

        var scope = AsyncScopedLifestyle.BeginScope(container);
        var service = scope.GetRequiredService<TService>();
        var result = new ServiceScope<TService>(service, [scope, connection, container]);

        return result;
    }
}
