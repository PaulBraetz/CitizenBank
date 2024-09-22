namespace CitizenBank.Features.Shared;
using System.Threading;
using System.Threading.Tasks;

using RhoMicro.ApplicationFramework.Common.Abstractions;

public sealed class CitizenNameInterceptor(IGetActualCitizenNameService nameService) : IInterceptor<CitizenName>
{
    public async ValueTask<CitizenName> Intercept(CitizenName obj, CancellationToken ct)
    {
        var loadResult = await nameService.GetActualCitizenName(obj, ct);
        var result = loadResult.Match(
            onCitizenName: n => n,
            onNotFound: _ => obj);

        return result;
    }
}
