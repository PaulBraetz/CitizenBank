namespace CitizenBank.Features.Authentication.CompleteRegistration;
using RhoMicro.ApplicationFramework.Aspects;
using CitizenBank.Features.Authentication;
using Microsoft.Extensions.Logging;

partial class CompleteRegistrationService(
    IValidatePasswordService validatePasswordService,
    IValidateBioService validateBioService,
    IDeleteRegistrationRequestService deleteRegistrationRequestService,
    IPersistRegistrationService persistRegistrationService,
    ILogger logger)
{
    [ServiceMethod]
    async ValueTask<CompleteRegistration.Result> CompleteRegistration(RegistrationRequest request, PrehashedPassword password, CancellationToken ct)
    {
        var validatePasswordResult = await validatePasswordService.ValidatePassword(password, request.Password, ct);
        if(validatePasswordResult.TryAsPasswordMismatch(out var passwordMismatch))
            return (CompleteRegistration.Failure)passwordMismatch;

        var validateBioCodeResult = await validateBioService.ValidateBio(request.Name, request.BioCode, ct);
        if(validateBioCodeResult.TryAsBioCodeMismatch(out var bioCodeMismatch))
            return (CompleteRegistration.Failure)bioCodeMismatch;
        if(validateBioCodeResult.TryAsUnknownCitizen(out var unknownCitizen))
            return (CompleteRegistration.Failure)unknownCitizen;

        var deleteRequestResult = await deleteRegistrationRequestService.DeleteRegistrationRequest(request.Name, ct);
        if(deleteRequestResult.TryAsDeleteRegistrationRequestFailure(out var deleteFailure))
            return (CompleteRegistration.Failure)deleteFailure;

        var persistRegistrationResult = await persistRegistrationService.PersistRegistration(request.Name, request.Password, ct);
        logger.LogWarning($"Completed registration for {request.Name}. TODO: identity integration");

        var result = persistRegistrationResult.Match<CompleteRegistration.Result>(
            s => (CompleteRegistration.Success)s,
            s => (CompleteRegistration.Success)s,
            f => (CompleteRegistration.Failure)f);

        return result;
    }
}
