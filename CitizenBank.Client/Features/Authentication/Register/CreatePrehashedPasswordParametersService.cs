namespace CitizenBank.Features.Authentication.Register;

using System.Security.Cryptography;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
partial class CreatePrehashedPasswordParametersService
{
    [ServiceMethod]
    static PrehashedPasswordParameters CreatePrehashedPasswordParameters() =>
        new(
            Salt: [.. RandomNumberGenerator.GetBytes(PrehashedPasswordMinimumParameters.SaltLength)],
            HashSize: PrehashedPasswordMinimumParameters.HashSize,
            Prf: PrehashedPasswordMinimumParameters.Prf,
            Iterations: 100);//PrehashedPasswordMinimumParameters.Iterations);
}
