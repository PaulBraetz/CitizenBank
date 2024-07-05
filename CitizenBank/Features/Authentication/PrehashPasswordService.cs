namespace CitizenBank.Features.Authentication;

using System.Collections.Immutable;
using System.Security.Cryptography;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;

using RhoMicro.ApplicationFramework.Aspects;

sealed partial class PrehashPasswordService
{
    private const Int32 _saltSize = 128 / 8;
    private const Int32 _hashSize = 256 / 8;
    private const KeyDerivationPrf _prf = KeyDerivationPrf.HMACSHA512;
    private const Int32 _iterations = 100000;

    static Byte[] HashToken(String token)
    {
        var salt = RandomNumberGenerator.GetBytes(_saltSize);
        var subkey = KeyDerivation.Pbkdf2(token, salt, _prf, _iterations, _hashSize);
        Byte[] outputBytes = [.. salt, .. subkey];
        return outputBytes;
    }

    [ServiceMethod]
    static PrehashedPassword PrehashPassword(ClearPassword clearPassword) => HashToken(clearPassword).ToImmutableArray();
}