namespace Tests.Unit;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using CitizenBank.Features.Authentication;

[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
public class ValidatePasswordServiceTests
{
    sealed class HashPasswordServiceImpl : IHashPasswordService
    {
        public ValueTask<HashedPassword> HashPassword(
            PrehashedPassword password,
            PasswordParameters parameters,
            CancellationToken cancellationToken) =>
            HashPasswordService.HashPassword(password, parameters);
    }
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
    public async Task ValidatePassword_yields_success_for_same_password(Int32 seed)
    {
        var (prehashedPassword, parameters) = PasswordHelpers.CreatePrehashedPasswordAndParameters(seed);
        var hashService = new HashPasswordServiceImpl();
        var hashedPassword = await hashService.HashPassword(prehashedPassword, parameters, default);
        var validatePasswordService = new ValidatePasswordService(hashService);
        var result = await validatePasswordService.ValidatePassword(prehashedPassword, hashedPassword, default);
        Assert.True(result.IsSuccess);
    }
    [Theory]
    [InlineData(1, 23)]
    [InlineData(1024, 6573)]
    [InlineData(512, 12321)]
    [InlineData(100, 879087)]
    [InlineData(123, 1)]
    [InlineData(12, 2)]
    [InlineData(2, 3)]
    [InlineData(0, 1)]
    [InlineData(Int32.MaxValue, 32)]
    public async Task ValidatePassword_yields_mismatch_for_different_password(Int32 seed, Int32 differentSeed)
    {
        var (prehashedPassword, parameters) = PasswordHelpers.CreatePrehashedPasswordAndParameters(seed);
        var hashService = new HashPasswordServiceImpl();
        var hashedPassword = await hashService.HashPassword(prehashedPassword, parameters, default);
        var validatePasswordService = new ValidatePasswordService(hashService);

        var (differentPrehashedPassword, differentParameters) = PasswordHelpers.CreatePrehashedPasswordAndParameters(differentSeed);

        Debug.Assert(!differentParameters.Equals(parameters));
        Debug.Assert(differentPrehashedPassword.Digest != prehashedPassword.Digest);

        var result = await validatePasswordService.ValidatePassword(differentPrehashedPassword, hashedPassword, default);
        Assert.True(result.IsMismatch);
    }
}
