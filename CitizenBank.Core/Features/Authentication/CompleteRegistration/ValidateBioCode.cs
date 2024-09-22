namespace CitizenBank.Features.Authentication.CompleteRegistration;

using CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.CodeAnalysis;

[FakeService]
partial class ValidateBioCodeServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "IValidateBioCodeService")]
    static ValidateBioCode.Result ValidateBioCode(CitizenName name, BioCode code) =>
        throw Exceptions.DefinitionNotSupported<ValidateBioCodeServiceDefinition>();
}

public partial record struct ValidateBioCode
{
    [UnionType<Success, GetCitizenBio.NotFound, Mismatch>]
    public readonly partial struct Result;
    public readonly struct Mismatch;
    public readonly struct Success;
}
