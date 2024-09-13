namespace Tests;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using CitizenBank.Features.Authentication;

using Konscious.Security.Cryptography;



[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
public class HashPasswordServiceTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(1024)]
    [InlineData(512)]
    [InlineData(100)]
    [InlineData(123)]
    [InlineData(12)]
    [InlineData(2)]
    [InlineData(0)]
    [InlineData(Int32.MaxValue)]
    public async Task HashPassword_hashes_using_argon_2id(Int32 seed)
    {
        var (prehashedPassword, parameters) = PasswordHelpers.CreatePrehashedPasswordAndParameters(seed);
        using var argon = new Argon2id(prehashedPassword)
        {
            AssociatedData = [.. parameters.Data.AssociatedData],
            KnownSecret = [.. parameters.Data.KnownSecret],
            Salt = [.. parameters.Data.Salt],
            Iterations = parameters.Numerics.Iterations,
            DegreeOfParallelism = parameters.Numerics.DegreeOfParallelism,
            MemorySize = parameters.Numerics.MemorySize,
        };
        var expectedHash = await argon.GetBytesAsync(parameters.Numerics.OutputLength);
        var actualHash = ( await HashPasswordService.HashPassword(prehashedPassword.ToImmutableArray(), parameters) ).Hash;

        Assert.Equal(expectedHash, actualHash);

    }

    [Theory]
    [InlineData(1)]
    public async Task HashPassword_includes_parameters_in_result(Int32 seed)
    {
        var (prehashedPassword, expectedParameters) = PasswordHelpers.CreatePrehashedPasswordAndParameters(seed);
        var actualParameters = ( await HashPasswordService.HashPassword(prehashedPassword.ToImmutableArray(), expectedParameters) ).Parameters;
        Assert.Equal(expectedParameters, actualParameters);
    }
}
