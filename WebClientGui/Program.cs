using CitizenBank.Composition;
using CitizenBank.Features.Authentication.Register.Client;

using Microsoft.Extensions.Options;

using RhoMicro.ApplicationFramework.Common.Environment;
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
    .AddApiServiceClients()
    .AddConsoleLogging()
    .ConfigureOptions(o =>
    {
        o.Composer = Composers.WebClient + o.Composer;
        o.OnContainerAdd += o =>
        {
            _ = o.Services
                .AddTransient<IPasswordGuideline>(sp =>
                    sp.GetRequiredService<IOptions<RegexPasswordGuideline>>().Value
                    ?? throw new InvalidOperationException($"Unable to resolve password guidelines."))
                .AddOptions<RegexPasswordGuideline>()
                .BindConfiguration("PasswordGuideline")
                .Validate(
                    o => true,
                    "")
                .ValidateOnStart();
        };
    })
    .ConfigureCapabilities(c => c.Components
        .Add(typeof(CitizenBank.Shared.Presentation.Views.App).Assembly)
        .Add(typeof(CitizenBank.WebClientGui.EntryPoint)))
    .Build()
    .RunAsync()
    .ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
#if DEBUG
} catch(Exception ex)
{
    Console.WriteLine(ex);
}
#endif