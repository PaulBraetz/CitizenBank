namespace CitizenBank.Features.Authentication.Login.Client;

using System.Threading.Tasks;

using CitizenBank.Features.Authentication.CompleteRegistration;
using CitizenBank.Features.Authentication.Login.Server;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common;

sealed partial class ClientLoginService(
    ILoadPrehashedPasswordParametersService parametersService,
    IPrehashPasswordService prehashPasswordService,
    IServerLoginService serverLoginService)
{
    [ServiceMethod]
    async ValueTask<ClientLogin.Result> ClientLogin(CitizenName name, ClearPassword password, CancellationToken ct)
    {
        var loadParametersResult = await parametersService.LoadPrehashedPasswordParameters(name, ct);
        if(loadParametersResult.IsNotFound)
            return new Failure("Citizen is not registered yet.");

        var prehashedPassword = await prehashPasswordService.PrehashPassword(password, loadParametersResult.AsPrehashedPasswordParameters, ct);
        var loginResult = await serverLoginService.ServerLogin(name, prehashedPassword, ct);

        var result = loginResult.Match(
            (ServerLogin.Failure f) => f.Match<ClientLogin.Result>(
                (CompleteRegistration.Failure f) => f.Match(
                    (LoadBio.UnknownCitizen _) => new Failure("Unknown citizen name."),
                    (Failure f) => f,
                    (ValidatePassword.Mismatch _) => new Failure("Password mismatch detected."),
                    (ValidateBioCode.Mismatch _) => new Failure("Bio does not contain required bio code.")),
                (Failure f) => f,
                (LoadRegistration.DoesNotExist _) => new Failure("Citizen is not registered yet."),
                (ValidatePassword.Mismatch m) => m,
                (ValidatePrehashedPasswordParameters.Insecure _) => new Failure("Insecure prehash parameters detected. This is likely due to an out of date client. In order to fix this, use the latest client and re-register your citizen.")),
            (ServerLogin.Success s) => new ClientLogin.Success());

        return result;
    }
}
