namespace CitizenBank.Features.Authentication;
using System;
using System.Linq;
using System.Threading.Tasks;

using RhoMicro.ApplicationFramework.Aspects;

partial class ValidatePasswordService(IHashPasswordService hashPasswordService)
{
    [ServiceMethodImplementation(Request = typeof(ValidatePassword), Service = typeof(IValidatePasswordService))]
    internal async ValueTask<ValidatePassword.Result> ValidatePassword(PrehashedPassword password, HashedPassword other, CancellationToken ct)
    {
        var hashed = await hashPasswordService.HashPassword(password, other.Parameters, ct);

        ValidatePassword.Result result = hashed.Digest == other.Digest
            ? new ValidatePassword.Success()
            : new ValidatePassword.Mismatch();

        return result;
    }
}
