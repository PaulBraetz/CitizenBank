namespace Tests.Integration;

using System.Threading;
using System.Threading.Tasks;

using CitizenBank.Features.Authentication.Login.Server;

using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
sealed class ServerLoginServiceMock(Func<ServerLogin, CancellationToken, ValueTask<ServerLogin.Result>> impl) : IService<ServerLogin, ServerLogin.Result>
{
    public ValueTask<ServerLogin.Result> Execute(ServerLogin request, CancellationToken cancellationToken) =>
        impl.Invoke(request, cancellationToken);
}
