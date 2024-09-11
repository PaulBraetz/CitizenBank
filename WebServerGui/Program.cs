using CitizenBank.Composition;
using CitizenBank.WebServerGui;

using Microsoft.Extensions.Options;

using RhoMicro.ApplicationFramework.Common.Environment;
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
    .AddBlazor()
    .AddApiServiceEndpoints()
    .AddConsoleLogging()
    .ConfigureOptions(o => o.Composer = Composers.WebServer + o.Composer)
    .ConfigureCapabilities(c =>
    {
        _ = c.Services
            .AddRazorComponents(options => options.DetailedErrors = true)
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
