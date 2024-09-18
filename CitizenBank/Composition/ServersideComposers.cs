namespace CitizenBank.Composition;

using CitizenBank.Features.Authentication;
using CitizenBank.Features.Authentication.CompleteRegistration;
using CitizenBank.Features.Authentication.Register;
using CitizenBank.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using RhoMicro.ApplicationFramework.Composition;

using SimpleInjector.Integration.ServiceCollection;

/// <summary>
/// Contains template composers.
/// </summary>
public static class ServersideComposers
{
    /// <summary>
    /// Gets the default composition root for web application servers.
    /// </summary>
    public static IComposer WebServer { get; } = CoreComposers.Instance;
    public static Action<SimpleInjectorAddOptions> SimpleinjectorAddHandler { get; } =
        o => o.Services
            .AddDbContext<CitizenBankContext>(b => b.UseSqlite(o.Container.GetRequiredService<IConfiguration>().GetConnectionString("CitizenBankContext")))
            .AddTransient<ILoadBioSettings>(sp =>
                sp.GetRequiredService<IOptions<LoadBioSettings>>().Value
                ?? throw new InvalidOperationException($"Unable to resolve bio load settings."))
            .AddOptions<LoadBioSettings>()
            .BindConfiguration("LoadBioSettings")
            .Validate(
                o => !String.IsNullOrEmpty(o.QueryUrlFormat),
                $"LoadBioSettings:{nameof(LoadBioSettings.QueryUrlFormat)} cannot be null or empty.")
            .ValidateOnStart()
            .Services
            .AddTransient<IDoesCitizenExistSettings>(sp =>
                sp.GetRequiredService<IOptions<DoesCitizenExistSettings>>().Value
                ?? throw new InvalidOperationException($"Unable to settings for checking if citizens exist."))
            .AddOptions<DoesCitizenExistSettings>()
            .BindConfiguration("DoesCitizenExistSettings")
            .Validate(
                o => !String.IsNullOrEmpty(o.QueryUrlFormat),
                $"DoesCitizenExistSettings:{nameof(DoesCitizenExistSettings.QueryUrlFormat)} cannot be null or empty.")
            .ValidateOnStart()
            .Services
            .AddHttpClient();
}
