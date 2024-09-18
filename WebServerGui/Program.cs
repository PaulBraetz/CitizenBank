using CitizenBank.Composition;
using CitizenBank.WebServerGui;

using Microsoft.Extensions.Options;

using RhoMicro.ApplicationFramework.Common.Environment;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.ApplicationFramework.Hosting;

await WebServerGuiApp.CreateBuilder(out var builder, s =>
    {
        s.BuilderSettings = new WebApplicationOptions()
        {
            Args = args,
#if DEBUG
            EnvironmentName = EnvironmentConfiguration.Development.Name
#else
            EnvironmentName = EnvironmentConfiguration.Production.Name
#endif
        };
    })
    .AddTimeout()
    .AddClipboard()
    .AddBlazor()
    .AddApiServiceEndpoints()
    .AddConsoleLogging()
    .ConfigureOptions(o =>
    {
        o.Composer += ClientsideComposers.Default + ServersideComposers.WebServer + Composer.Create(c =>
            c.RegisterServices(
                ConventionalServiceRegistrationOptions.PreferAssembly(typeof(ServersideComposers).Assembly)
#if !DEBUG
                    with { RegistrationPredicate = ConventionalServiceRegistrationPredicates.IgnoreAttributeFakes }
#endif
                ,
                typeof(CoreComposers).Assembly,
                typeof(ServersideComposers).Assembly));

        o.OnContainerAdd += CoreComposers.SimpleinjectorAddHandler;
        o.OnContainerAdd += ServersideComposers.SimpleinjectorAddHandler;
        o.OnContainerAdd += o => o.Services
            .AddOptions<CorsSettings>()
            .BindConfiguration("CorsSettings")
            .Validate(
                s => s.AllowedOrigins.All(o => !String.IsNullOrWhiteSpace(o)),
                $"The configuration key CorsSettings:{nameof(CorsSettings.AllowedOrigins)} cannot have empty or null entries.")
            .ValidateOnStart()
            .Services
            .AddCors();
    })
    .ConfigureCapabilities(c =>
    {
        _ = c.Services.AddRazorComponents(options => options.DetailedErrors = true)
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();
        _ = c.Components
            .Add(typeof(CitizenBank.Shared.Presentation.Views.App).Assembly)
            .Add(typeof(CitizenBank.WebClientGui.EntryPoint))
            .Add(typeof(CitizenBank.WebServerGui.App));
    })
    .Build()
    .ConfigureUnderlyingApp((app, container) =>
    {
        // Configure the HTTP request pipeline.
        if(app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        } else
        {
            _ = app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            _ = app.UseHsts();
        }

        var allowedOrigins = app.Services.GetRequiredService<IOptions<CorsSettings>>().Value.AllowedOrigins;
        _ = app.UseCors(b => b.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());

        _ = app.UseHttpsRedirection();

        _ = app.UseStaticFiles();
        _ = app.UseAntiforgery();

        _ = app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(
                typeof(CitizenBank.Shared.Presentation.Views.App).Assembly,
                typeof(CitizenBank.WebClientGui.EntryPoint).Assembly);
    })
    .MapApiServiceEndpoints()
    .RunAsync()
    .ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
