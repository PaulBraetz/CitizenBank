﻿namespace CitizenBank.Features.Authentication.CompleteRegistration;
using RhoMicro.ApplicationFramework.Aspects;
using CitizenBank.Features.Authentication;
using RhoMicro.ApplicationFramework.Common;

sealed partial class ValidateBioService(
    ILoadBioService loadBioService)
{
    [ServiceMethod]
    async ValueTask<ValidateBioCode.Result> ValidateBio(CitizenName name, BioCode code, CancellationToken ct)
    {
        var loadBioResult = await loadBioService.LoadBio(name, ct);
        if(loadBioResult.TryAsUnknownCitizen(out var unknownCitizen))
            return unknownCitizen;

        ValidateBioCode.Result result = loadBioResult.AsBio.Content.Contains(code.Value, StringComparison.InvariantCulture)
            ? new Success()
            : new ValidateBioCode.BioCodeMismatch();

        return result;
    }
}
