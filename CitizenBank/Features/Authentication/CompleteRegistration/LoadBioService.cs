namespace CitizenBank.Features.Authentication.CompleteRegistration;

using System.Globalization;

using CitizenBank.Infrastructure;

using RhoMicro.ApplicationFramework.Aspects;

partial class LoadBioService(HttpClientAccessor clientAccessor, ILoadBioSettings settings)
{
    [ServiceMethodImplementation(Request = typeof(LoadBio), Service = typeof(ILoadBioService))]
    async ValueTask<LoadBio.Result> LoadBio(CitizenName name, CancellationToken ct)
    {
        var uriString = String.Format(CultureInfo.InvariantCulture, settings.QueryUrlFormat, name.AsString);
        var uri = new Uri(uriString);
        var response = await clientAccessor.Client.GetAsync(uri, ct);
        LoadBio.Result result = response.IsSuccessStatusCode
            ? new Bio(await response.Content.ReadAsStringAsync(ct))
            : new LoadBio.UnknownCitizen();

        return result;
    }
}
