namespace CitizenBank.Features.Authentication.Login.Client;
using System.Threading.Tasks;

using CitizenBank.Features.Authentication.Login.Server;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common;

sealed partial class ClientLoginService(
    IPrehashPasswordService prehashPasswordService,
    IServerLoginService serverLoginService)
{
    [ServiceMethod]
    async ValueTask<ClientLogin.Result> ClientLogin(CitizenName name, ClearPassword password, CancellationToken ct)
    {
        var prehashedPassword = await prehashPasswordService.PrehashPassword(password, ct);
        var loginResult = await serverLoginService.ServerLogin(name, prehashedPassword, ct);

        var result = loginResult.Match(
            f => f.Match<ClientLogin.Result>(
                f => new Failure(),
                f => new Failure(),
                f => new Failure(),
                (ValidatePassword.PasswordMismatch f) => f),
            s => new Success());

        return result;
    }
}
