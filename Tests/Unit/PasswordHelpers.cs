namespace Tests.Unit;

using System.Diagnostics.CodeAnalysis;

using CitizenBank.Features.Authentication;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;

static class PasswordHelpers
{
    [SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "<Pending>")]
    public static (PrehashedPassword password, PasswordParameters parameters) CreatePrehashedPasswordAndParameters(Int32 seed)
    {
        var random = new Random(seed);
        var associatedData = getRandomBytes();
        var knownSecret = getRandomBytes();
        var salt = getRandomBytes();
        var prehashSalt = getRandomBytes();
        var prehashedPassword = getRandomBytes();
        var parameters = new PasswordParameters(
            new PasswordParameterNumerics(
                Iterations: 10,
                DegreeOfParallelism: 2,
                MemorySize: 8,
                OutputLength: 512),
            new PasswordParameterData(
                AssociatedData: [.. associatedData],
                KnownSecret: [.. knownSecret],
                Salt: [.. salt]));
        var prehashedParameters = new PrehashedPasswordParameters(
            Salt: [.. prehashSalt],
            HashSize: 512,
            Prf: KeyDerivationPrf.HMACSHA512,
            Iterations: 100);

        return (new([.. prehashedPassword], prehashedParameters), parameters);

        Byte[] getRandomBytes()
        {
            var result = new Byte[512];
            random.NextBytes(result);
            return result;
        }
    }

}