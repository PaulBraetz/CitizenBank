namespace CitizenBank.Features.Authentication;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common;

using static CitizenBank.Features.Authentication.ValidatePrehashedPasswordParameters;

partial class ValidatePrehashedPasswordParametersService
{
    [ServiceMethod]
    static ValidatePrehashedPasswordParameters.Result ValidatePrehashedPasswordParameters(PrehashedPasswordParameters parameters) =>
        parameters.HashSize >= PrehashedPasswordDefaultParameters.HashSize
        && parameters.Iterations >= PrehashedPasswordDefaultParameters.Iterations
        && parameters.Prf >= PrehashedPasswordDefaultParameters.Prf
        && parameters.Salt.Length >= PrehashedPasswordDefaultParameters.SaltLength
        ? new Success()
        : new Insecure();
}
