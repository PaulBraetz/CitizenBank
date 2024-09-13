namespace CitizenBank.Features.Authentication.CompleteRegistration;
using RhoMicro.ApplicationFramework.Aspects;
using CitizenBank.Features.Authentication;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
sealed partial class ValidateBioService(
    ILoadBioService loadBioService)
{
    [ServiceMethod]
    async ValueTask<ValidateBioCode.Result> ValidateBio(CitizenName name, BioCode code, CancellationToken ct)
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
