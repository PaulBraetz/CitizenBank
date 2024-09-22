namespace CitizenBank.Features.Shared;
using System.Threading;
using System.Threading.Tasks;

using RhoMicro.ApplicationFramework.Aspects;

partial class GetCitizenImagePathService(ILoadCitizenProfilePageService profilePageService) : IGetCitizenImagePathService
{
    [ServiceMethodImplementation(Request = typeof(GetCitizenImagePath), Service = typeof(IGetCitizenImagePathService))]
    public async ValueTask<GetCitizenImagePath.Result> GetCitizenImagePath(CitizenName name, CancellationToken ct)
    {
        var profile = await profilePageService.LoadCitizenProfilePage(name, ct);
        var result = profile.Match<GetCitizenImagePath.Result>(
            onCitizenProfilePage: p => p.ImagePath,
            onNotFound: _ => new GetCitizenImagePath.NotFound());

        return result;
    }
}