namespace Tests.Integration;

using System.Threading;
using System.Threading.Tasks;

using CitizenBank.Features.Authentication.Login.Server;

using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
sealed class LoadPrehashedPasswordParametersServiceMock(Func<LoadPrehashedPasswordParameters, CancellationToken, ValueTask<LoadPrehashedPasswordParameters.Result>> impl) :
    IService<LoadPrehashedPasswordParameters, LoadPrehashedPasswordParameters.Result>
{
    public ValueTask<LoadPrehashedPasswordParameters.Result> Execute(LoadPrehashedPasswordParameters request, CancellationToken cancellationToken) =>
        impl.Invoke(request, cancellationToken);
}
