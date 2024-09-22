﻿namespace CitizenBank.Features.Authentication.Login;

using CitizenBank.Features.Shared;
using CitizenBank.Persistence;

using Microsoft.EntityFrameworkCore;

using RhoMicro.ApplicationFramework.Aspects;

partial class LoadPrehashedPasswordParametersService(CitizenBankContext context)
{
    [ServiceMethodImplementation(Request = typeof(LoadPrehashedPasswordParameters), Service = typeof(ILoadPrehashedPasswordParametersService))]
    async ValueTask<LoadPrehashedPasswordParameters.Result> LoadPrehashedPasswordParameters([Intercept] CitizenName name, PrehashedPasswordParametersSource source, CancellationToken ct)
    {
        var nameString = name.Value;
        LoadPrehashedPasswordParameters.Result result =
            source switch
            {
                PrehashedPasswordParametersSource.RegistrationRequest =>
                   ( await context.RegistrationRequests.SingleOrDefaultAsync(r => r.Name == nameString, ct) )?
                       .Password,
                PrehashedPasswordParametersSource.Registration =>
                   ( await context.Registrations.SingleOrDefaultAsync(r => r.Name == nameString, ct) )?
                       .Password,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source, $"Unable to handle prehashed password parameters source '{source}'.")
            } switch
            {
                { } pw => pw.PrehashedPasswordParameters.ToPrehashedPasswordParameters(),
                null => new LoadPrehashedPasswordParameters.NotFound()
            };

        return result;
    }
}