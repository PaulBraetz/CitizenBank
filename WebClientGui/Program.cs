using CitizenBank.Composition;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using RhoMicro.ApplicationFramework.Common.Environment;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.ApplicationFramework.Hosting;

#if DEBUG
var environmentConfiguration = EnvironmentConfiguration.Development;
#else
var environmentConfiguration = EnvironmentConfiguration.Production;
#endif
var client = new HttpClient()
{
    BaseAddress = new Uri(WebAssemblyHostBuilder.CreateDefault(args).HostEnvironment.BaseAddress)
};
var appsettingsPath = $"appsettings.{environmentConfiguration.Name}.Core.json";
var appsettingsUri = new Uri(appsettingsPath, UriKind.Relative);
var appsettingsStream = await client.GetStreamAsync(appsettingsUri);
await WebClientGuiApp.CreateBuilder(out var builder, s =>
    {
        s.Args = args;
        s.EnvironmentConfiguration = environmentConfiguration;
        s.SetupLoggingCallback = Console.WriteLine;
    })
    //get config up before mapping clients, as those resolve settings
    .ConfigureCapabilities(c => c.Configuration.AddJsonStream(appsettingsStream))
    .AddTimeout()
    .AddBlazor()
    .AddApiServiceClients()
    .AddConsoleLogging()
    .ConfigureOptions(o =>
    {
        o.Composer += ClientsideComposers.WebClient + Composer.Create(c =>
        c.RegisterServices(
            ConventionalServiceRegistrationOptions.PreferAssemblies(typeof(ClientsideComposers).Assembly)
#if !DEBUG
                with { RegistrationPredicate = ConventionalServiceRegistrationPredicates.IgnoreAttributeFakes }
#endif
            ,
            typeof(CoreComposers).Assembly,
            typeof(ClientsideComposers).Assembly));
        o.OnContainerAdd += CoreComposers.SimpleinjectorAddHandler;
        o.OnContainerAdd += ClientsideComposers.SimpleinjectorAddHandler;
    })
    .ConfigureCapabilities(c =>
    {
        _ = c.Components
            .Add(typeof(CitizenBank.Features.Shared.Views.App).Assembly)
            .Add(typeof(CitizenBank.WebClientGui.EntryPoint));
    })
    .Build()
    .ConfigureUnderlyingApp((app, container) =>
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
           Console.Error.WriteLine($"Fatal Exception: {args.ExceptionObject}");
    })
    .RunAsync()
    .ConfigureAwait(true);
