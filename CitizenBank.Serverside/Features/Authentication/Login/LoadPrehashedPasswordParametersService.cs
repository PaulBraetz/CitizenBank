namespace CitizenBank.Features.Authentication.Login;

using CitizenBank.Features.Shared;
using CitizenBank.Persistence;

using Microsoft.EntityFrameworkCore;

using RhoMicro.ApplicationFramework.Aspects;

partial class LoadPrehashedPasswordParametersService(CitizenBankContext context)
{
    [ServiceMethodImplementation(Request = typeof(LoadPrehashedPasswordParameters), Service = typeof(ILoadPrehashedPasswordParametersService))]
    async ValueTask<LoadPrehashedPasswordParameters.Result> LoadPrehashedPasswordParameters([Intercept] CitizenName name, LoginType loginType, CancellationToken ct)
    {
        var nameString = name.Value;
        LoadPrehashedPasswordParameters.Result result =
            loginType switch
            {
                LoginType.CompleteRegistration =>
                   ( await context.RegistrationRequests.FindAsync([nameString], ct) )?
                       .Password,
                LoginType.Regular =>
                   ( await context.Registrations.FindAsync([nameString], ct) )?
                       .Password,
                _ => throw new ArgumentOutOfRangeException(nameof(loginType), loginType, $"Unable to handle login type '{loginType}'.")
            } switch
            {
                { } pw => pw.PrehashedPasswordParameters.ToPrehashedPasswordParameters(),
                null => loginType switch
                {
                    LoginType.CompleteRegistration => (LoadPrehashedPasswordParameters.Failure)new LoadPrehashedPasswordParameters.RegistrationRequestNotFound(),
                    LoginType.Regular => (LoadPrehashedPasswordParameters.Failure)new LoadPrehashedPasswordParameters.RegistrationNotFound(),
                    _ => throw new ArgumentOutOfRangeException(nameof(loginType), loginType, $"Unable to handle login type '{loginType}'.")
                }
            };

        return result;
    }
}
