using CitizenBank.Composition;

using RhoMicro.ApplicationFramework.Common.Environment;
using RhoMicro.ApplicationFramework.Hosting;

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
    .AddApiServiceClients()
    .ConfigureOptions(o => o.Composer = Composers.WebGuiClient + o.Composer)
    .ConfigureCapabilities(c => c.Components
        .Add(typeof(CitizenBank.Shared.Presentation.Views.App).Assembly)
        .Add(typeof(CitizenBank.WebClientGui.EntryPoint)))
    .Build()
    .RunAsync()
    .ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
