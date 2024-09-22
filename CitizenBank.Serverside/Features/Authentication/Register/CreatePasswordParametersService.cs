namespace CitizenBank.Features.Authentication.Register;

using System.Security.Cryptography;

using RhoMicro.ApplicationFramework.Aspects;

partial class CreatePasswordParametersService
{
    [ServiceMethodImplementation(Request = typeof(CreatePasswordParameters), Service = typeof(ICreatePasswordParametersService))]
    static PasswordParameters CreatePasswordParameters() =>
        new(
            new PasswordParameterNumerics(
                Iterations: HashedPasswordMinimumParameters.Iterations,
                DegreeOfParallelism: HashedPasswordMinimumParameters.DegreeOfParallelism,
                MemorySize: HashedPasswordMinimumParameters.MemorySize,
                OutputLength: HashedPasswordMinimumParameters.OutputLength),
            new PasswordParameterData(
                AssociatedData: RandomNumberGenerator.GetBytes(
                    HashedPasswordMinimumParameters.AssociatedDataLength),
                KnownSecret: RandomNumberGenerator.GetBytes(
                    HashedPasswordMinimumParameters.KnownSecretLength),
                Salt: RandomNumberGenerator.GetBytes(
                    HashedPasswordMinimumParameters.SaltLength)));
}
