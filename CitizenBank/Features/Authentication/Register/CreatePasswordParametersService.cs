namespace CitizenBank.Features.Authentication.Register;

using System.Security.Cryptography;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
partial class CreatePasswordParametersService
{
    [ServiceMethodImplementation(Request = typeof(CreatePasswordParameters), Service = typeof(ICreatePasswordParametersService))]
    static PasswordParameters CreatePasswordParameters() =>
        new(
            new PasswordParameterNumerics(
                Iterations: 2,//128,
                DegreeOfParallelism: 4,
                MemorySize: 1024 * 8,
                OutputLength: 1024),
            new PasswordParameterData(
                AssociatedData: [.. RandomNumberGenerator.GetBytes(512)],
                KnownSecret: [.. RandomNumberGenerator.GetBytes(512)],
                Salt: [.. RandomNumberGenerator.GetBytes(512)]));
}
