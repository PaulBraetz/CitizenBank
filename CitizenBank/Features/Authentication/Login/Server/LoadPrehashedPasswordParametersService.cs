namespace CitizenBank.Features.Authentication.Login.Server;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common;

sealed partial class LoadPrehashedPasswordParametersService(DbFake db)
{
    [ServiceMethod]
    public LoadPrehashedPasswordParameters.Result LoadPrehashedPasswordParameters(CitizenName name)
    {
        LoadPrehashedPasswordParameters.Result result = db.Registrations.TryGetValue(name, out var registration)
            ? registration.Password.PrehashedPasswordParameters
            : db.RegistrationRequests.TryGetValue(name, out var registrationRequest)
                ? registrationRequest.Password.PrehashedPasswordParameters
                : new LoadPrehashedPasswordParameters.NotFound();

        return result;
    }
}
