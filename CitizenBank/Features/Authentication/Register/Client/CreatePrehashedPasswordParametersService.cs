namespace CitizenBank.Features.Authentication.Register.Client;

using System.Security.Cryptography;

using CitizenBank.Features.Authentication;

using RhoMicro.ApplicationFramework.Aspects;

sealed partial class CreatePrehashedPasswordParametersService
{
    [ServiceMethod]
    static PrehashedPasswordParameters CreatePrehashedPasswordParameters() =>
        new(
            Salt: [.. RandomNumberGenerator.GetBytes(PrehashedPasswordDefaultParameters.SaltLength)],
            HashSize: PrehashedPasswordDefaultParameters.HashSize,
            Prf: PrehashedPasswordDefaultParameters.Prf,
            Iterations: PrehashedPasswordDefaultParameters.Iterations);
}
