namespace CitizenBank.Features.Shared;
using System.Threading;
using System.Threading.Tasks;

using RhoMicro.ApplicationFramework.Aspects;

partial class GetActualCitizenNameService(ILoadCitizenProfilePageService profilePageService) : IGetActualCitizenNameService
{
    [ServiceMethodImplementation(Request = typeof(GetActualCitizenName), Service = typeof(IGetActualCitizenNameService))]
    public async ValueTask<GetActualCitizenName.Result> GetActualCitizenName(CitizenName name, CancellationToken ct)
    {
        var profile = await profilePageService.LoadCitizenProfilePage(name, ct);
        var result = profile.Match< GetActualCitizenName.Result>(
            onCitizenProfilePage: p => p.Name,
            onNotFound: _ => new GetActualCitizenName.NotFound());

        return result;
    }
}
