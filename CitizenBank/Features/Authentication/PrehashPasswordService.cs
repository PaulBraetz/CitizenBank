namespace CitizenBank.Features.Authentication;

using System.Collections.Immutable;
using System.Security.Cryptography;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Cryptography.KeyDerivation.PBKDF2;

using RhoMicro.ApplicationFramework.Aspects;

sealed partial class PrehashPasswordService(YieldingManagedPbkdf2Provider pbkdf2Provider)
{
    private const Int32 _saltSize = 128 / 8;
    private const Int32 _hashSize = 256 / 8;
    private const KeyDerivationPrf _prf = KeyDerivationPrf.HMACSHA512;
    private const Int32 _iterations = 10000;

    async ValueTask<Byte[]> HashToken(String token, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var salt = RandomNumberGenerator.GetBytes(_saltSize);
        var subkey = await pbkdf2Provider.DeriveKey(
            token,
            salt,
            _prf,
            _iterations,
            _hashSize,
            ct);

        Byte[] outputBytes = [.. salt, .. subkey];

        return outputBytes;
    }

    [ServiceMethod]
    async ValueTask<PrehashedPassword> PrehashPassword(
        ClearPassword clearPassword,
        CancellationToken ct) =>
        ( await HashToken(clearPassword, ct) ).ToImmutableArray();
}
