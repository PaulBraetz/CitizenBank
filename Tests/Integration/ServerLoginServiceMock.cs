namespace Tests.Integration;

using System.Threading;
using System.Threading.Tasks;

using CitizenBank.Features.Authentication;
using CitizenBank.Features.Authentication.Login;
using CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Composition;

[FakeService]
sealed class ServerLoginServiceMock(Func<CitizenName, PrehashedPassword, CancellationToken, ValueTask<ServerLogin.Result>> impl) : IServerLoginService
{
    public ServerLoginServiceMock(ServerLogin.Result result) : this((_, _, _) => ValueTask.FromResult(result)) { }
    public ValueTask<ServerLogin.Result> ServerLogin(CitizenName name, PrehashedPassword password, CancellationToken cancellationToken = default) =>
        impl.Invoke(name, password, cancellationToken);
}
