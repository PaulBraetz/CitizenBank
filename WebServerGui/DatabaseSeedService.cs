namespace CitizenBank.WebServerGui;
using Bogus;

using CitizenBank.Features.Authentication;
using CitizenBank.Features.Authentication.Login;
using CitizenBank.Features.Authentication.Register;
using CitizenBank.Persistence;

using SimpleInjector;
using SimpleInjector.Lifestyles;

class DatabaseSeedService(Container container) : IHostedService
{
    class CredentialsDto(String name)
    {
        public CredentialsDto() : this(_names[Interlocked.Increment(ref _count) % _names.Length])
        { }

        static Int32 _count;
        static readonly String[] _names = [
            "majboomer",
            "brazenO66",
            "kymbold",
            "dinoteeth",
            "enhancedhd",
            "ace"];
        public static Int32 MaxCount => _names.Length;
        public String Name { get; } = name;
        public required String Password { get; set; }
    }
    public async Task StartAsync(CancellationToken ct)
    {
        var scope1 = AsyncScopedLifestyle.BeginScope(container);
        await using(scope1.ConfigureAwait(true))
        {
            var context = scope1.GetRequiredService<CitizenBankContext>();
            _ = await context.Database.EnsureDeletedAsync(ct).ConfigureAwait(true);
            _ = await context.Database.EnsureCreatedAsync(ct).ConfigureAwait(true);
        }

        var seedTasks = new Faker<CredentialsDto>()
            .UseSeed(1)
            .RuleFor(d => d.Password, f => f.Internet.Password())
            .Generate(count: CredentialsDto.MaxCount)
            .Append(new("SleepWellPupper") { Password = "Password1." })
            .ToList()
            .Select(async d =>
            {
                var scope2 = AsyncScopedLifestyle.BeginScope(container);
                await using(scope2.ConfigureAwait(false))
                {
                    var clientRegisterService = scope2.GetRequiredService<IClientRegisterService>();
                    var registerResult = await clientRegisterService
                        .ClientRegister(d.Name, d.Password, ct)
                        .ConfigureAwait(false);
                    
                    if(!( registerResult.IsCreateSuccess || registerResult.IsOverwriteSuccess ))
                    {
                        var logger = scope2.GetRequiredService<ILoggerFactory>().CreateLogger<DatabaseSeedService>();
                        logger.LogError("register result for citizen {Name} does not indicate success: {Result}", d.Name, registerResult);
                    }
                    //}

                    //var scope3 = AsyncScopedLifestyle.BeginScope(container);
                    //await using(scope3.ConfigureAwait(false))
                    //{
                    var clientLoginService = scope2.GetRequiredService<IClientLoginService>();
                    var loginResult = await clientLoginService
                        .ClientLogin(d.Name, d.Password, PrehashedPasswordParametersSource.RegistrationRequest, ct)
                        .ConfigureAwait(false);

                    if(!loginResult.IsSuccess)
                    {
                        var logger = scope2.GetRequiredService<ILoggerFactory>().CreateLogger<DatabaseSeedService>();
                        logger.LogError("login result for citizen {Name} does not indicate success: {Result}", d.Name, loginResult);
                    }
                }
            });

        await Task.WhenAll(seedTasks).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}