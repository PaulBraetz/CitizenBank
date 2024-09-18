namespace CitizenBank.Features.Authentication.CompleteRegistration;
using RhoMicro.ApplicationFramework.Aspects;
using CitizenBank.Features.Authentication;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
partial class ValidateBioCodeService(ILoadBioService loadBioService)
{
    [ServiceMethodImplementation(Request = typeof(ValidateBioCode), Service = typeof(IValidateBioCodeService))]
    async ValueTask<ValidateBioCode.Result> ValidateBioCode(CitizenName name, BioCode code, CancellationToken ct)
    {
        return new ValidateBioCode.Success();

        var loadBioResult = await loadBioService.LoadBio(name, ct);
        if(loadBioResult.TryAsUnknownCitizen(out var unknownCitizen))
            return unknownCitizen;

        ValidateBioCode.Result result = loadBioResult.AsBio!.Content.Contains(code.Value, StringComparison.InvariantCulture)
            ? new ValidateBioCode.Success()
            : new ValidateBioCode.Mismatch();

        return result;
    }
}
