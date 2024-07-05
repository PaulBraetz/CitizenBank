namespace CitizenBank.LocalGui;

using System;

using CitizenBank.Composition;
using CitizenBank.Features.Authentication.Register.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NReco.Logging.File;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.ApplicationFramework.Common.Environment;
using RhoMicro.ApplicationFramework.Hosting;

class Program
{
    [STAThread]
    static void Main(String[] args) =>
        LocalGuiApp.CreateBuilder(out var builder, s =>
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
        .AddAppSettings()
        .AddApiServiceClients()
        .ConfigureBuilder(b => b.RootComponents.Add<EntryPoint>("app"))
        .ConfigureOptions(o =>
        {
            o.Composer = Composers.LocalGui + o.Composer;
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
        .ConfigureCapabilities(c =>
        {
            var loggingSection = c.Configuration.Build().GetSection("Logging");
            _ = c.Logging.AddFile(loggingSection);
            _ = c.Components
                .Add(typeof(Shared.Presentation.Views.App).Assembly)
                .Add(typeof(EntryPoint));
        })
        .Build()
        .ConfigureUnderlyingApp((app, container) =>
        {
            var isDevToolsEnabled = container
                .GetInstance<IEnvironmentConfiguration>()
                .IsDevelopment();

            _ = app.MainWindow
                .SetIconFile("favicon.ico")
                .SetTitle("Photino Blazor Sample")
                .SetDevToolsEnabled(isDevToolsEnabled);

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                app.MainWindow.ShowMessage("Fatal exception", args.ExceptionObject.ToString());
        })
        .Run();
}
