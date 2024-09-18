namespace CitizenBank.LocalGui;

using System;

using CitizenBank.Composition;

using NReco.Logging.File;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.ApplicationFramework.Common.Environment;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.ApplicationFramework.Hosting;

using SimpleInjector;

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
        .AddApiServiceClients(o => { })
        .AddFileLogging()
        .ConfigureBuilder(b => b.RootComponents.Add<EntryPoint>("app"))
        .ConfigureOptions(o =>
        {
            o.Composer += ClientsideComposers.LocalClient + Composer.Create(c =>
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
                .SetTitle("CitizenBank")
                .SetDevToolsEnabled(isDevToolsEnabled);

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                app.MainWindow.ShowMessage("Fatal exception", args.ExceptionObject.ToString());
        })
        .Run();
}
