namespace CitizenBank.Features.Authentication;
using System;
using System.Linq;
using System.Threading.Tasks;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common;

sealed partial class ValidatePasswordService(IHashPasswordService hashPasswordFactory)
{
    [ServiceMethod]
    async ValueTask<ValidatePassword.Result> ValidatePassword(PrehashedPassword password, HashedPassword other, CancellationToken ct)
    {
        var hashed = await hashPasswordFactory.HashPassword(password, other.Parameters, ct);

        ValidatePassword.Result result = hashed.Hash.SequenceEqual(other.Hash)
            ? new Success()
            : new ValidatePassword.PasswordMismatch();

        return result;
    }
}
