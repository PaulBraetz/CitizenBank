namespace CitizenBank.Features.Authentication.Login;

using System.Threading.Tasks;

using CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common;

partial class ClientLoginService(
    IDoesCitizenExistService doesCitizenExistService,
    ILoadPrehashedPasswordParametersService parametersService,
    IPrehashPasswordService prehashPasswordService,
    IServerLoginService serverLoginService)
{
    [ServiceMethodImplementation(Request = typeof(ClientLogin), Service = typeof(IClientLoginService))]
    async ValueTask<ClientLogin.Result> ClientLogin(
        CitizenName name,
        ClearPassword password,
        PrehashedPasswordParametersSource parametersSource,
        CancellationToken ct)
    {
        var existsResult = await doesCitizenExistService.DoesCitizenExist(name, ct);
        if(existsResult.TryAsDoesNotExist(out var doesNotExist))
            return doesNotExist;

        var loadParametersResult = await parametersService.LoadPrehashedPasswordParameters(name, parametersSource, ct);
        if(loadParametersResult.IsNotFound)
            return new Failure("Unable to locate registration or registration request foir citizen.");

        var prehashedPassword = await prehashPasswordService.PrehashPassword(password, loadParametersResult.AsPrehashedPasswordParameters, ct);
        var loginResult = await serverLoginService.ServerLogin(name, prehashedPassword, ct);

        var result = loginResult.Match(
            (ServerLogin.Failure f) => f.Match<ClientLogin.Result>(
                (CompleteRegistration.CompleteRegistration.Failure f) => f.Match(
                    (GetCitizenBio.NotFound _) => new Failure("Unknown citizen name."),
                    (Failure f) => f,
                    (ValidatePassword.Mismatch _) => new Failure("Password mismatch detected."),
                    (CompleteRegistration.ValidateBioCode.Mismatch _) => new Failure("Bio does not contain required bio code.")),
                (Failure f) => f,
                (LoadRegistration.DoesNotExist _) => new Failure("Citizen is not registered yet."),
                (ValidatePassword.Mismatch m) => m,
                (ValidatePrehashedPasswordParameters.Insecure _) => new Failure("Insecure prehash parameters detected. This is likely due to an out of date client. In order to fix this, use the latest client and re-register your citizen."),
                (DoesCitizenExist.DoesNotExist d) => d),
            (ServerLogin.Success s) => new ClientLogin.Success());

        return result;
    }
}
