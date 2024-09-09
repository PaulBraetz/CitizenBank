namespace CitizenBank.Features.Authentication.Register.Server;

using System.Security.Cryptography;

using CitizenBank.Features.Authentication;

using RhoMicro.ApplicationFramework.Aspects;

sealed partial class CreatePasswordParametersService
{
    [ServiceMethod]
    static PasswordParameters CreatePasswordParameters() => 
        new(
            new PasswordParameterNumerics(
                Iterations: 128,
                DegreeOfParallelism: 4,
                MemorySize: 1024 * 8,
                OutputLength: 1024),
            new PasswordParameterData(
                AssociatedData: [.. RandomNumberGenerator.GetBytes(64)],
                KnownSecret: [.. RandomNumberGenerator.GetBytes(64)],
                Salt: [.. RandomNumberGenerator.GetBytes(64)]));
}