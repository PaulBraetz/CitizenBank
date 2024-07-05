namespace CitizenBank.Features.Authentication;

using System.Threading.Tasks;

using Konscious.Security.Cryptography;

using RhoMicro.ApplicationFramework.Aspects;

sealed partial class HashPasswordService
{
    [ServiceMethod]
    static async ValueTask<HashedPassword> HashPassword(PrehashedPassword password, PasswordParameters parameters)
    {
        var clearBytes = password.Match(onImmutableArray_of_Byte: b => b.ToArray());
        using var argon = new Argon2id(clearBytes)
        {
            DegreeOfParallelism = parameters.Numerics.DegreeOfParallelism,
            Iterations = parameters.Numerics.Iterations,
            MemorySize = parameters.Numerics.MemorySize,
            Salt = [.. parameters.Data.Salt],
            KnownSecret = [.. parameters.Data.KnownSecret],
            AssociatedData = [.. parameters.Data.AssociatedData]
        };
        var hash = await argon.GetBytesAsync(parameters.Numerics.OutputLength);
        var result = new HashedPassword([.. hash], parameters);

        return result;
    }
}