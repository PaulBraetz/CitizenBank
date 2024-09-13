namespace CitizenBank.Features.Authentication;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;

static class PrehashedPasswordDefaultParameters
{
    public const Int32 SaltLength = 64;
    public const Int32 HashSize = 64;
    public const KeyDerivationPrf Prf = KeyDerivationPrf.HMACSHA512;
    public const Int32 Iterations = 10000;
}