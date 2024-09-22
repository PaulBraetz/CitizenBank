namespace CitizenBank.Features.Shared;

using System.Threading.Tasks;

using RhoMicro.ApplicationFramework.Aspects;

partial class DoesCitizenExistService(ILoadCitizenProfilePageService profilePageService)
{

    [ServiceMethodImplementation(Request = typeof(DoesCitizenExist), Service = typeof(IDoesCitizenExistService))]
    async ValueTask<DoesCitizenExist.Result> DoesCitizenExist(CitizenName name, CancellationToken ct)
    {
        var loadResult = await profilePageService.LoadCitizenProfilePage(name, ct);
        var result = loadResult.Match<DoesCitizenExist.Result>(
            onCitizenProfilePage: _ => new DoesCitizenExist.Success(),
            onNotFound: _ => new DoesCitizenExist.DoesNotExist());

        return result;
    }
}
