namespace Tests.Integration;

using System.Diagnostics;

using CitizenBank.Composition;
using CitizenBank.Persistence;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;

using RhoMicro.ApplicationFramework.Composition;

using SimpleInjector;
using SimpleInjector.Lifestyles;
public abstract class IntegrationTestBase
{
    protected sealed class ServiceScope<TService>(TService service, IEnumerable<IDisposable> disposables) : IDisposable
    {
        public TService Service { get; } = service;

        public void Dispose()
        {
            foreach(var d in disposables)
                d.Dispose();
        }
    }
    static Int32 _dbIndex;
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "disposed via returned scope")]
    protected static ServiceScope<TService> CreateService<TService>(params (Type serviceType, Object instance)[] mockRegistrations)
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
                            ["LoadCitizenProfilePageSettings:QueryUrlFormat"] = "https://robertsspaceindustries.com/citizens/{0}",
                            ["LoadCitizenProfilePageSettings:ProfilePageSettings:BioPath"] = "//span[@class='label'][contains(text(), 'Bio')]/following-sibling::div[1]",
                            ["LoadCitizenProfilePageSettings:ProfilePageSettings:NamePath"] = "//span[@class='label'][contains(text(), 'Handle name')]/following-sibling::strong[1]",
                            ["LoadCitizenProfilePageSettings:ProfilePageSettings:ImagePath"] = "//span[@class='title'][contains(text(), 'Profile')]/following-sibling::div/div[@class='thumb']/img[1]",
                            ["LoadCitizenProfilePageSettings:ProfilePageSettings:ImageBasePath"] = "https://robertsspaceindustries.com",
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
            Composer.Create(c =>
            {
                c.RegisterServices(
                    ConventionalServiceRegistrationOptions.PreferAssemblies(
                        typeof(ClientsideComposers).Assembly,
                        typeof(ServersideComposers).Assembly)
                    with
                    {
                        RegistrationPredicate = info =>
                            !mocksMap.ContainsKey(info.TraditionalServiceType)
                    },
                    typeof(CoreComposers).Assembly,
                    typeof(ClientsideComposers).Assembly,
                    typeof(ServersideComposers).Assembly);

                c.Register<IJSRuntime, FakeJsRuntime>(Lifestyle.Singleton);
                c.RegisterInstance<ILogger>(NullLogger.Instance);
                c.RegisterConditional<HttpClientAccessor, HttpClientAccessorFake>(ctx => true);

                foreach(var (service, instance) in mocksMap)
                    c.RegisterInstance(service, instance);
            }) +
            PresentationComposers.Models +
            CoreComposers.Instance +
            ClientsideComposers.LocalClient;

        composer.Compose(container);

        var connection = new SqliteConnection(connectionString);
        connection.Open();

        container.Verify(VerificationOption.VerifyAndDiagnose);

        using(var setupScope = AsyncScopedLifestyle.BeginScope(container))
        {
            var context = setupScope.GetInstance<CitizenBankContext>();
            _ = context.Database.EnsureDeleted();
            _ = context.Database.EnsureCreated();
        }

        var scope = AsyncScopedLifestyle.BeginScope(container);

        var accessor = scope.GetService<HttpClientAccessor>();
        if(accessor is not null or HttpClientAccessorFake)
            throw new InvalidOperationException("Registered http client accessor is not fake; this would allow for outbound http requests in tests.");

        var service = scope.GetRequiredService<TService>();
        var result = new ServiceScope<TService>(service, [scope, connection, container]);

        return result;
    }
}
