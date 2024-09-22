namespace CitizenBank.Features.Shared;
using CitizenBank.Persistence;
using System.Globalization;

using RhoMicro.CodeAnalysis;
using RhoMicro.ApplicationFramework.Aspects;

partial class LoadCitizenProfilePageService(HttpClientAccessor clientAccessor, ILoadCitizenProfilePageSettings settings)
{
    [ServiceMethod]
    async ValueTask<LoadCitizenProfilePage.Result> LoadCitizenProfilePage(CitizenName name, CancellationToken ct)
    {
        var queryString = String.Format(CultureInfo.InvariantCulture, settings.QueryUrlFormat, name.Value);
        var queryUri = new Uri(queryString, UriKind.Absolute);
        var response = await clientAccessor.Client.GetAsync(queryUri, ct);
        if(!response.IsSuccessStatusCode)
            return new LoadCitizenProfilePage.NotFound();

        var html = await response.Content.ReadAsStringAsync(ct);
        var result = new CitizenProfilePage(html, settings.ProfilePageSettings);

        return result;
    }
}

partial record struct LoadCitizenProfilePage
{
    [UnionType<CitizenProfilePage, NotFound>]
    public readonly partial struct Result;
    public readonly struct NotFound;
}