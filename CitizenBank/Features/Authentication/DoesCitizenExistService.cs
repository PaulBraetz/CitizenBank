namespace CitizenBank.Features.Authentication;

using System.Globalization;
using System.Threading.Tasks;

using CitizenBank.Infrastructure;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common;

partial class DoesCitizenExistService(HttpClientAccessor clientAccessor, IDoesCitizenExistSettings settings)
{

    [ServiceMethodImplementation(Request = typeof(DoesCitizenExist), Service = typeof(IDoesCitizenExistService))]
    async ValueTask<DoesCitizenExist.Result> DoesCitizenExist(CitizenName name, CancellationToken ct)
    {
        var uriString = String.Format(CultureInfo.InvariantCulture, settings.QueryUrlFormat, name.AsString);
        var uri = new Uri(uriString);
        var response = await clientAccessor.Client.GetAsync(uri, ct);
        DoesCitizenExist.Result result = response.IsSuccessStatusCode
            ? new Success()
            : new DoesCitizenExist.DoesNotExist();

        return result;
    }
}
