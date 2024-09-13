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

        ValidatePassword.Result result = hashed.Hash.SequenceEqual(other.Hash)
            ? new ValidatePassword.Success()
            : new ValidatePassword.Mismatch();

        return result;
    }
}
