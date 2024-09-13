﻿namespace CitizenBank.Features.Authentication;

using System.Collections.Immutable;
using System.Security.Cryptography;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Cryptography.KeyDerivation.PBKDF2;

using RhoMicro.ApplicationFramework.Aspects;

public sealed partial class PrehashPasswordService(YieldingManagedPbkdf2Provider pbkdf2Provider)
{
    [ServiceMethod]
    public async ValueTask<PrehashedPassword> PrehashPassword(
        ClearPassword clearPassword,
        PrehashedPasswordParameters parameters,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var subkey = await pbkdf2Provider.DeriveKey(
            clearPassword,
            parameters,
            ct);
        Byte[] bytes = [.. parameters.Salt, .. subkey];
        var result = new PrehashedPassword([.. bytes], parameters);

        return result;
    }
}
