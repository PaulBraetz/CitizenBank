namespace Tests;

using System.Diagnostics.CodeAnalysis;

using CitizenBank.Features.Authentication;

static class PasswordHelpers
{
    [SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "<Pending>")]
    public static (Byte[] prehashedPassword, PasswordParameters parameters) CreatePrehashedPasswordAndParameters(Int32 seed)
    {
        var random = new Random(seed);
        var associatedData = getRandomBytes();
        var knownSecret = getRandomBytes();
        var salt = getRandomBytes();
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

        return (prehashedPassword, parameters);

        Byte[] getRandomBytes()
        {
            var result = new Byte[512];
            random.NextBytes(result);
            return result;
        }
    }

}