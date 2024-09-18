namespace CitizenBank.Features.Authentication;
using Microsoft.AspNetCore.Cryptography.KeyDerivation.PBKDF2;

using RhoMicro.ApplicationFramework.Aspects;

partial class PrehashPasswordService(YieldingManagedPbkdf2Provider pbkdf2Provider)
{
    [ServiceMethod]
    async ValueTask<PrehashedPassword> PrehashPassword(
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
