namespace CitizenBank.Features.Authentication.Login.Server;

using System.Diagnostics;
using System.Threading.Tasks;

using CitizenBank.Features.Authentication.CompleteRegistration;

using Microsoft.Extensions.Logging;

using RhoMicro.ApplicationFramework.Aspects;

public sealed partial class ServerLoginService(
    IValidatePrehashedPasswordParametersService validateParametersService,
    ILoadRegistrationRequestService loadRegistrationRequestService,
    ICompleteRegistrationService completeRegistrationService,
    ILoadRegistrationService loadRegistrationService,
    IValidatePasswordService validatePasswordService,
    ILogger logger)
{
    [ServiceMethod]
    async ValueTask<ServerLogin.Result> ServerLogin(CitizenName name, PrehashedPassword password, CancellationToken ct)
    {
        var validateParametersResult = await validateParametersService.ValidatePrehashedPasswordParameters(password.Parameters, ct);
        if(validateParametersResult.TryAsInsecure(out var f))
            return (ServerLogin.Failure)f;

        var loadRequestResult = await loadRegistrationRequestService.LoadRegistrationRequest(name, ct);

        if(loadRequestResult.TryAsRegistrationRequest(out var registrationRequest))
        {
            var completionResult = await completeRegistrationService.CompleteRegistration(registrationRequest, password, ct);
            if(completionResult.TryAsFailure(out var completionFailure))
                return (ServerLogin.Failure)completionFailure;
        }

        var loadRegistrationResult = await loadRegistrationService.LoadRegistration(name, ct);
        if(loadRegistrationResult.TryAsDoesNotExist(out var registrationDoesNotExist))
            return (ServerLogin.Failure)registrationDoesNotExist;

        Debug.Assert(loadRegistrationResult.AsRegistration != null);

        var validateResult = await validatePasswordService.ValidatePassword(password, loadRegistrationResult.AsRegistration.Password, ct);
        if(validateResult.TryAsMismatch(out var passwordMismatch))
            return (ServerLogin.Failure)passwordMismatch;

        //identity integration here
        //TODO: update sequence diagram
        logger.LogWarning($"Authenticated {name}. TODO: identity integration");

        return new ServerLogin.Success();
    }
}
