namespace Tests.Integration;

using System.Threading;
using System.Threading.Tasks;

using CitizenBank.Features.Authentication.Register.Server;

using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
sealed class ServerRegisterServiceMock(Func<ServerRegister, CancellationToken, ValueTask<ServerRegister.Result>> impl) : IService<ServerRegister, ServerRegister.Result>
{
    public ValueTask<ServerRegister.Result> Execute(ServerRegister request, CancellationToken cancellationToken) => 
        impl.Invoke(request, cancellationToken);
}