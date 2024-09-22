namespace CitizenBank.Features.Authentication;

using System.Security.Cryptography;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;

public static class PrehashedPasswordMinimumParameters
{
    public const Int32 SaltLength = 512;
    public const Int32 HashSize = 512;
    public const KeyDerivationPrf Prf = KeyDerivationPrf.HMACSHA512;
    public const Int32 Iterations = 10000;
}
