namespace CitizenBank.Features.Authentication;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.CodeAnalysis;

[FakeService]
partial class ValidatePasswordServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "IValidatePasswordService")]
    static ValidatePassword.Result ValidatePassword(PrehashedPassword password, HashedPassword other) =>
        throw Exceptions.DefinitionNotSupported<ValidatePasswordServiceDefinition>();
}

partial record struct ValidatePassword
{
    [UnionType<Success, Mismatch>]
    public readonly partial struct Result;
    public readonly struct Mismatch;
    public readonly struct Success;
}