namespace Tests.Integration;

using System.Threading;
using System.Threading.Tasks;

using CitizenBank.Features.Authentication;
using CitizenBank.Features.Authentication.Register;
using CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Composition;

[FakeService]
sealed class ServerRegisterServiceMock(Func<CitizenName, PrehashedPassword, CancellationToken, ValueTask<ServerRegister.Result>> impl) : IServerRegisterService
{
    public ServerRegisterServiceMock(ServerRegister.Result result) : this((_, _, _) => ValueTask.FromResult(result)) { }

    public ValueTask<ServerRegister.Result> ServerRegister(CitizenName name, PrehashedPassword password, CancellationToken cancellationToken = default) =>
        impl.Invoke(name, password, cancellationToken);
}