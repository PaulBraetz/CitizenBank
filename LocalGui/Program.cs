namespace CitizenBank.LocalGui;

using System;

using CitizenBank.Composition;

using Microsoft.Extensions.Configuration;

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
    static void Main(String[] args)
    {
        LocalGuiApp.CreateBuilder(s =>
            {
                s.Args = args;
                s.SetupLoggingCallback = Console.WriteLine;
#if DEBUG
                s.EnvironmentConfiguration = EnvironmentConfiguration.Development;
#else
                s.EnvironmentConfiguration = EnvironmentConfiguration.Production;
#endif
            })
           //get config up before mapping clients, as those resolve settings
           .ConfigureCapabilities(c => c.Configuration.AddJsonFile(Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? String.Empty, $"appsettings.{c.EnvironmentConfiguration.Name}.Core.json")))
           .AddFileLogging(configSection: "Logging:File")
           //.AddTimeout()
           .AddBlazor()
           .AddAspects(Lifestyle.Scoped, CommonAspects.Execution)
           .AddAppSettings()
           .AddApiServiceClients()
           .ConfigureBuilder(b => b.RootComponents.Add<EntryPoint>("app"))
           .ConfigureOptions(o =>
           {
               o.Composer += ClientsideComposers.LocalClient + Composer.Create(c =>
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
               var loggingSection = c.Configuration.Build().GetSection("Logging");
               _ = c.Logging.AddFile(loggingSection);
               _ = c.Components
               .Add(typeof(Features.Shared.Views.App).Assembly)
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
}
