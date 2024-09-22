namespace CitizenBank.Composition;

using CitizenBank.Features.Shared;
using CitizenBank.Persistence;

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
            .AddTransient<ILoadCitizenProfilePageSettings>(sp =>
                sp.GetRequiredService<IOptions<LoadCitizenProfilePageSettings>>().Value
                ?? throw new InvalidOperationException($"Unable to citizen page load settings."))
            .AddOptions<LoadCitizenProfilePageSettings>()
            .BindConfiguration("LoadCitizenProfilePageSettings")
            .Validate(
                o => !String.IsNullOrEmpty(o.QueryUrlFormat),
                $"LoadCitizenProfilePageSettings:{nameof(LoadCitizenProfilePageSettings.QueryUrlFormat)} cannot be null or empty.")
            .Validate(
                o => !String.IsNullOrEmpty(o.ProfilePageSettings?.NamePath),
                $"LoadCitizenProfilePageSettings:{nameof(LoadCitizenProfilePageSettings.ProfilePageSettings)}:{nameof(CitizenProfilePageSettings.NamePath)} cannot be null or empty.")
            .Validate(
                o => !String.IsNullOrEmpty(o.ProfilePageSettings?.BioPath),
                $"LoadCitizenProfilePageSettings:{nameof(LoadCitizenProfilePageSettings.ProfilePageSettings)}:{nameof(CitizenProfilePageSettings.BioPath)} cannot be null or empty.")
            .Validate(
                o => !String.IsNullOrEmpty(o.ProfilePageSettings?.ImagePath),
                $"LoadCitizenProfilePageSettings:{nameof(LoadCitizenProfilePageSettings.ProfilePageSettings)}:{nameof(CitizenProfilePageSettings.ImagePath)} cannot be null or empty.")
            .Validate(
                o => !String.IsNullOrEmpty(o.ProfilePageSettings?.ImageBasePath),
                $"LoadCitizenProfilePageSettings:{nameof(LoadCitizenProfilePageSettings.ProfilePageSettings)}:{nameof(CitizenProfilePageSettings.ImageBasePath)} cannot be null or empty.")
            .ValidateOnStart()
            .Services
            .AddHttpClient();
}
