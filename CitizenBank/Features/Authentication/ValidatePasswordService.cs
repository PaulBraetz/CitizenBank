namespace CitizenBank.Features.Authentication;
using System;
using System.Linq;
using System.Threading.Tasks;

using RhoMicro.ApplicationFramework.Aspects;

public sealed partial class ValidatePasswordService(IHashPasswordService hashPasswordService)
{
    [ServiceMethod]
    public async ValueTask<ValidatePassword.Result> ValidatePassword(PrehashedPassword password, HashedPassword other, CancellationToken ct)
    {
        var hashed = await hashPasswordService.HashPassword(password, other.Parameters, ct);

        ValidatePassword.Result result = hashed.Digest.SequenceEqual(other.Digest)
            ? new ValidatePassword.Success()
            : new ValidatePassword.Mismatch();

        return result;
    }
}
