namespace CitizenBank.Features.Authentication.Login;

using System.Diagnostics;
using System.Threading.Tasks;

using CitizenBank.Features.Authentication.CompleteRegistration;
using CitizenBank.Features.Shared;

using Microsoft.Extensions.Logging;

using RhoMicro.ApplicationFramework.Aspects;

partial class ServerLoginService(
    IDoesCitizenExistService doesCitizenExistService,
    IValidatePrehashedPasswordParametersService validateParametersService,
    ILoadRegistrationRequestService loadRegistrationRequestService,
    ICompleteRegistrationService completeRegistrationService,
    ILoadRegistrationService loadRegistrationService,
    IValidatePasswordService validatePasswordService,
    ILogger logger)
{
    [ServiceMethodImplementation(Request = typeof(ServerLogin), Service = typeof(IServerLoginService))]
    async ValueTask<ServerLogin.Result> ServerLogin([Intercept] CitizenName name, PrehashedPassword password, CancellationToken ct)
    {
        var existResult = await doesCitizenExistService.DoesCitizenExist(name, ct);
        if(existResult.TryAsDoesNotExist(out var doesNotExist))
            return (ServerLogin.Failure)doesNotExist;

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
        logger.LogWarning("Authenticated {Name}. TODO: identity integration", name);

        return new ServerLogin.Success();
    }
}
