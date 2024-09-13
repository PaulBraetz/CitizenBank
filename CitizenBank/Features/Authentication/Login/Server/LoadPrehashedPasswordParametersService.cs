namespace CitizenBank.Features.Authentication.Login.Server;

using CitizenBank.Features.Authentication.Infrastructure;

using RhoMicro.ApplicationFramework.Aspects;

public enum PrehashedPasswordParametersSource
{
    RegistrationRequest,
    Registration
}

public sealed partial class LoadPrehashedPasswordParametersService(CitizenBankContext context)
{
    [ServiceMethod]
    public LoadPrehashedPasswordParameters.Result LoadPrehashedPasswordParameters(CitizenName name, PrehashedPasswordParametersSource source)
    {
        var nameString = name.AsString;
        LoadPrehashedPasswordParameters.Result result =
            source switch
            {
                PrehashedPasswordParametersSource.RegistrationRequest =>
                   context.RegistrationRequests.SingleOrDefault(r => r.Name == nameString)?
                       .Password,
                PrehashedPasswordParametersSource.Registration =>
                   context.Registrations.SingleOrDefault(r => r.Name == nameString)?
                       .Password,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source, $"Unable to handle prehashed password parameters source '{source}'.")
            } switch
            {
                { } pw => pw.PrehashedPasswordParameters.ToPrehashedPasswordParameters(),
                null => new LoadPrehashedPasswordParameters.NotFound()
            };

        return result;
    }
}
