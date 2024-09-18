using CitizenBank.Composition;

using RhoMicro.ApplicationFramework.Common.Environment;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.ApplicationFramework.Hosting;

#if DEBUG
try
{
#endif
    await WebClientGuiApp.CreateBuilder(out var builder, s =>
    {
        s.Args = args;
#if DEBUG
        s.EnvironmentConfiguration = EnvironmentConfiguration.Development;
#else
        s.EnvironmentConfiguration = EnvironmentConfiguration.Production;
#endif
    })
    .AddTimeout()
    .AddBlazor()
    .AddApiServiceClients(o => { })
    .AddConsoleLogging()
    .ConfigureOptions(o =>
    {
        o.Composer += ClientsideComposers.WebClient + Composer.Create(c =>
            c.RegisterServices(
                ConventionalServiceRegistrationOptions.PreferAssembly(typeof(ClientsideComposers).Assembly)
#if !DEBUG
                    with { RegistrationPredicate = ConventionalServiceRegistrationPredicates.IgnoreAttributeFakes }
#endif
                ,
                typeof(CoreComposers).Assembly,
                typeof(ClientsideComposers).Assembly));

        o.OnContainerAdd += CoreComposers.SimpleinjectorAddHandler;
        o.OnContainerAdd += ClientsideComposers.SimpleinjectorAddHandler;
    })
    .ConfigureCapabilities(c => c.Components
        .Add(typeof(CitizenBank.Shared.Presentation.Views.App).Assembly)
        .Add(typeof(CitizenBank.WebClientGui.EntryPoint)))
    .Build()
    .RunAsync()
    .ConfigureAwait(true);
#if DEBUG
} catch(Exception ex)
{
    Console.WriteLine(ex);
}
#endif