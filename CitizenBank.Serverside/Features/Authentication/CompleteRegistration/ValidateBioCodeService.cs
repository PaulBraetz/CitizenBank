namespace CitizenBank.Features.Authentication.CompleteRegistration;
using RhoMicro.ApplicationFramework.Aspects;
using CitizenBank.Features.Authentication;
using RhoMicro.ApplicationFramework.Composition;
using CitizenBank.Features.Shared;

partial class ValidateBioCodeService(IGetCitizenBioService bioService)
{
    [ServiceMethodImplementation(Request = typeof(ValidateBioCode), Service = typeof(IValidateBioCodeService))]
    async ValueTask<ValidateBioCode.Result> ValidateBioCode(CitizenName name, BioCode code, CancellationToken ct)
    {
#if DEBUG
        return new ValidateBioCode.Success();
#endif
        var loadBioResult = await bioService.GetCitizenBio(name, ct);
        if(loadBioResult.TryAsNotFound(out var notFound))
            return notFound;

        ValidateBioCode.Result result = loadBioResult.AsCitizenBio.Value.Contains(code.Value, StringComparison.InvariantCulture)
            ? new ValidateBioCode.Success()
            : new ValidateBioCode.Mismatch();

        return result;
    }
}
