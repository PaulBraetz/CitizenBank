namespace CitizenBank.Features.Shared;
using System.Threading;
using System.Threading.Tasks;

using RhoMicro.ApplicationFramework.Aspects;

partial class GetCitizenBioService(ILoadCitizenProfilePageService profilePageService) : IGetCitizenBioService
{
    [ServiceMethodImplementation(Request = typeof(GetCitizenBio), Service = typeof(IGetCitizenBioService))]
    public async ValueTask<GetCitizenBio.Result> GetCitizenBio(CitizenName name, CancellationToken ct)
    {
        var profile = await profilePageService.LoadCitizenProfilePage(name, ct);
        var result = profile.Match<GetCitizenBio.Result>(
            onCitizenProfilePage: p => p.Bio,
            onNotFound: _ => new GetCitizenBio.NotFound());

        return result;
    }
}
