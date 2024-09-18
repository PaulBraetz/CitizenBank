namespace CitizenBank.Features.Authentication;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.CodeAnalysis;

[FakeService]
partial class ValidatePrehashedPasswordParametersServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "IValidatePrehashedPasswordParametersService")]
    static ValidatePrehashedPasswordParameters.Result ValidatePrehashedPasswordParameters(PrehashedPasswordParameters parameters) =>
        throw Exceptions.DefinitionNotSupported<ValidatePrehashedPasswordParametersServiceDefinition>();
}

partial record struct ValidatePrehashedPasswordParameters
{
    [UnionType<Success, Insecure>]
    public readonly partial struct Result;
    public readonly struct Insecure;
}